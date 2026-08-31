# CI 质量门禁

`.github/workflows/dotnet.yml` 在 `master` push 和所有 pull_request 上运行：

## 后端

```bash
dotnet restore
dotnet build --no-restore --configuration Release
dotnet test --no-build --configuration Release
dotnet list src/DFApp.Web/DFApp.Web.csproj package --vulnerable --include-transitive
```

## 前端

```bash
pnpm install --frozen-lockfile
pnpm typecheck
pnpm lint:check
pnpm build
```

发布 Job 依赖以上两个检查，仅在 `master` push 时创建预览 Release，pull_request 不会发布。
