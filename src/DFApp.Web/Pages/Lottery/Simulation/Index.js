$(function () {
    // 定义模态框对象
    var l = abp.localization.getResource('DFApp');
    var dataTable = null;

    // 创建数据表格
    function createDataTable() {
        dataTable = $('#SimulationTable').DataTable(
            abp.libs.datatables.normalizeConfiguration({
                serverSide: true,
                paging: true,
                order: [[0, "asc"]],
                searching: false,
                scrollX: true,
                ajax: abp.libs.datatables.createAjax(dFApp.lottery.simulation.lotterySimulation.getList),
                columnDefs: [
                    {
                        title: l('LotterySimulation:TermNumber'),
                        data: "termNumber",
                        className: 'text-start'
                    },
                    {
                        title: l('LotterySimulation:Number'),
                        data: "number",
                        className: 'text-start'
                    },
                    {
                        title: l('LotterySimulation:BallType'),
                        data: "ballType",
                        className: 'text-start',
                        render: function (data) {
                            return data === 0 ? l('LotterySimulation:BallType:Red') : l('LotterySimulation:BallType:Blue');
                        }
                    },
                    {
                        title: l('LotterySimulation:GameType'),
                        data: "gameType",
                        className: 'text-start',
                        render: function (data) {
                            return data === 0 ? '双色球' : '快乐8';
                        }
                    },
                    {
                        title: l('LotterySimulation:GroupId'),
                        data: "groupId",
                        className: 'text-start'
                    }
                ]
            })
        );
    }

    // 生成随机号码按钮点击事件
    $('#GenerateRandomNumbersButton').click(function (e) {
        var modal = new abp.ModalManager({
            viewUrl: '/Lottery/Simulation/GenerateRandomNumbersModal'
        });
        modal.open();
    });

    // 计算中奖按钮点击事件
    $('#CalculateWinningButton').click(function (e) {
        var modal = new abp.ModalManager({
            viewUrl: '/Lottery/Simulation/CalculateWinningModal'
        });
        modal.open();
    });

    $('#StatisticsButton').click(function (e) {
        e.preventDefault();
        window.location.href = '/Lottery/Simulation/Statistics';
    });

    // 初始化页面
    createDataTable();
});
