#!/bin/bash

# 电动车成本跟踪功能测试脚本

BASE_URL="https://localhost:44369"

echo "====================================="
echo "电动车成本跟踪功能测试"
echo "====================================="
echo ""

# 1. 获取访问令牌
echo "1. 获取访问令牌..."
TOKEN=$(curl -k -s -X POST "${BASE_URL}/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=DFApp_Web&client_secret=X!*l}4Ab[K~um%I*#2&username=test&password=1q2w3E*" \
  | jq -r '.access_token')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
  echo "❌ 获取令牌失败"
  exit 1
fi

echo "✅ 获取令牌成功"
echo ""

# 2. 获取车辆列表
echo "2. 获取车辆列表..."
curl -k -s -X GET "${BASE_URL}/api/app/electric-vehicle" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.totalCount, .items[].{name, brand, totalMileage}' 2>/dev/null || echo "暂无车辆"
echo ""

# 3. 创建车辆
echo "3. 创建新车..."
VEHICLE_RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/app/electric-vehicle" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "测试车辆",
    "brand": "比亚迪",
    "model": "汉EV",
    "licensePlate": "京A88888",
    "batteryCapacity": 85.4,
    "totalMileage": 10000,
    "remark": "测试车辆"
  }')

VEHICLE_ID=$(echo $VEHICLE_RESPONSE | jq -r '.id')
VEHICLE_NAME=$(echo $VEHICLE_RESPONSE | jq -r '.name')

if [ "$VEHICLE_ID" != "null" ] && [ -n "$VEHICLE_ID" ]; then
  echo "✅ 车辆创建成功: $VEHICLE_NAME (ID: $VEHICLE_ID)"
else
  echo "❌ 车辆创建失败"
  echo $VEHICLE_RESPONSE | jq .
fi
echo ""

# 4. 创建成本记录
echo "4. 创建成本记录..."
if [ "$VEHICLE_ID" != "null" ] && [ -n "$VEHICLE_ID" ]; then
  COST_RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/app/electric-vehicle-cost" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
      \"vehicleId\": \"$VEHICLE_ID\",
      \"costType\": 1,
      \"costDate\": \"$(date +%Y-%m-%d)\",
      \"amount\": 500.00,
      \"isBelongToSelf\": true,
      \"remark\": \"充电费用\"
    }")

  COST_ID=$(echo $COST_RESPONSE | jq -r '.id')

  if [ "$COST_ID" != "null" ] && [ -n "$COST_ID" ]; then
    echo "✅ 成本记录创建成功 (ID: $COST_ID)"
  else
    echo "❌ 成本记录创建失败"
  fi
else
  echo "⚠️  跳过成本记录创建（车辆ID无效）"
fi
echo ""

# 5. 创建充电记录
echo "5. 创建充电记录..."
if [ "$VEHICLE_ID" != "null" ] && [ -n "$VEHICLE_ID" ]; then
  CHARGING_RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/app/electric-vehicle-charging-record" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
      \"vehicleId\": \"$VEHICLE_ID\",
      \"chargingDate\": \"$(date +%Y-%m-%d)\",
      \"stationName\": \"国家电网充电站\",
      \"chargingDuration\": 60,
      \"energy\": 45.5,
      \"amount\": 150.00,
      \"startSOC\": 20,
      \"endSOC\": 80,
      \"isBelongToSelf\": true
    }")

  CHARGING_ID=$(echo $CHARGING_RESPONSE | jq -r '.id')

  if [ "$CHARGING_ID" != "null" ] && [ -n "$CHARGING_ID" ]; then
    echo "✅ 充电记录创建成功 (ID: $CHARGING_ID)"
  else
    echo "❌ 充电记录创建失败"
  fi
else
  echo "⚠️  跳过充电记录创建（车辆ID无效）"
fi
echo ""

# 6. 获取成本记录列表
echo "6. 获取成本记录列表..."
curl -k -s -X GET "${BASE_URL}/api/app/electric-vehicle-cost" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.totalCount, .items[] | {costDate, amount, costType}' 2>/dev/null || echo "暂无成本记录"
echo ""

# 7. 获取油价信息
echo "7. 获取油价信息..."
GASOLINE_RESPONSE=$(curl -k -s -X GET "${BASE_URL}/api/app/gasoline-price/latest-price?province=山东" \
  -H "Authorization: Bearer $TOKEN")

echo $GASOLINE_RESPONSE | jq '. | {province, date, price92H, price95H, price98H}' 2>/dev/null || echo "暂无油价数据"
echo ""

# 8. 油电对比
echo "8. 油电对比分析..."
COMPARISON_RESPONSE=$(curl -k -s -X GET "${BASE_URL}/api/app/electric-vehicle-cost/oil-cost-comparison?startDate=$(date -d '30 days ago' +%Y-%m-%d)&endDate=$(date +%Y-%m-%d)" \
  -H "Authorization: Bearer $TOKEN")

echo $COMPARISON_RESPONSE | jq '. | {
  electricVehicleTotalCost,
  electricVehicleMileage,
  electricVehicleCostPerKm,
  oilVehicleTotalCost,
  savings,
  savingsPercentage
}' 2>/dev/null || echo "暂无对比数据"
echo ""

# 9. 清理测试数据
echo "9. 清理测试数据..."

if [ "$COST_ID" != "null" ] && [ -n "$COST_ID" ]; then
  curl -k -s -X DELETE "${BASE_URL}/api/app/electric-vehicle-cost/$COST_ID" \
    -H "Authorization: Bearer $TOKEN" >/dev/null 2>&1
  echo "✅ 成本记录已删除"
fi

if [ "$CHARGING_ID" != "null" ] && [ -n "$CHARGING_ID" ]; then
  curl -k -s -X DELETE "${BASE_URL}/api/app/electric-vehicle-charging-record/$CHARGING_ID" \
    -H "Authorization: Bearer $TOKEN" >/dev/null 2>&1
  echo "✅ 充电记录已删除"
fi

if [ "$VEHICLE_ID" != "null" ] && [ -n "$VEHICLE_ID" ]; then
  curl -k -s -X DELETE "${BASE_URL}/api/app/electric-vehicle/$VEHICLE_ID" \
    -H "Authorization: Bearer $TOKEN" >/dev/null 2>&1
  echo "✅ 车辆已删除"
fi

echo ""
echo "====================================="
echo "测试完成！"
echo "====================================="
echo ""
echo "访问地址："
echo "  前端: http://localhost:8848"
echo "  后端 Swagger: ${BASE_URL}/swagger"
echo "  油电对比页面: http://localhost:8848/electric-vehicle/statistics"
echo ""
