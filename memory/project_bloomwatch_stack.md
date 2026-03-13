---
name: BloomWatch tech stack and architecture
description: Core stack and architectural conventions for the BloomWatch project
type: project
---

BloomWatch is a shared anime tracker platform for pairs/small groups.

**Why:** Useful for all future implementation tasks to understand conventions upfront.

**How to apply:** Follow these conventions when implementing new modules.

Stack:
- Backend: .NET 10, ASP.NET Core, minimal APIs
- ORM: EF Core 9 + Npgsql (PostgreSQL)
- Architecture: Modular monolith, DDD-inspired (Domain/Application/Infrastructure/Contracts per module)
- Database: PostgreSQL, one schema per module (identity, watch_spaces, anime_tracking, analytics, anilist_sync)
- Auth: JWT HS256 via `BloomWatch.Modules.Identity`
- Solution file: `BloomWatch.slnx` (NOT .sln)
- Test stack: xUnit, NSubstitute, FluentAssertions; integration tests use SQLite in-memory via `WebApplicationFactory`
- Module registration: extension method `Add<ModuleName>Module(IServiceCollection, IConfiguration)` from Infrastructure
- Modules go under `src/Modules/<Name>/BloomWatch.Modules.<Name>.{Domain,Application,Infrastructure,Contracts}/`
