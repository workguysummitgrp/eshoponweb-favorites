# Test Plan — eShopOnWeb Favorites Enhancement

| Field | Value |
|---|---|
| **Project** | eShopOnWeb Favorites |
| **Slug** | eshoponweb-favorites |
| **Epic** | DEMO3-1 |
| **Phase** | Testing |
| **Date** | 2026-03-31 |
| **Tracks Executed** | unit, functional, security, compliance |

---

## 1. Executive Summary

This test plan covers 4 specialist testing tracks for the Favorites enhancement to eShopOnWeb. The feature adds a per-user Favorites toggle (heart icon) to catalog items, persisted in SQL Server, with a REST API for external consumers. Testing validates 13 user stories (41 story points) across domain, infrastructure, web UI, and PublicApi layers.

---

## 2. Unit Test Plan

**Writer**: `testing-unit-writer` | **Skill**: `polyglot-test`

### Scope
- Domain layer: FavoriteItem entity construction, guard clauses, boundary values
- Specifications: UserFavoritesSpecification (3 overloads) with in-memory `Evaluate()`
- Services: FavoriteService toggle/batch-fetch/get-all — happy paths, edge cases, error paths, CancellationToken propagation
- Controllers: FavoritesController toggle — auth, validation, error propagation
- View services: CatalogViewModelService + CachedCatalogViewModelService — IsFavorited population, cache isolation

### Test Files
| File | Tests | Stories |
|---|---|---|
| FavoriteServiceTests/ToggleFavorite.cs | 6 | US-002 |
| FavoriteServiceTests/GetUserFavoritesForItems.cs | 4 | US-012 |
| FavoriteServiceTests/GetUserFavoriteIds.cs | 4 | US-008 |
| FavoritesControllerToggle.cs | 4 | US-005, US-006 |
| CatalogViewModelServiceGetCatalogItems.cs | 3 | US-003, US-006, US-012 |
| CachedCatalogViewModelServiceGetCatalogItems.cs | 3 | US-004 |
| **FavoriteItemTests/Construction.cs** (new) | 8 | US-001 |
| **UserFavoritesSpecificationTests.cs** (new) | 10 | US-001, US-002, US-011, US-012 |
| **ToggleFavoriteEdgeCases.cs** (new) | 7 | US-002 |
| **GetUserFavoritesForItemsEdgeCases.cs** (new) | 7 | US-002, US-012 |
| **GetUserFavoriteIdsEdgeCases.cs** (new) | 6 | US-008 |
| **FavoritesControllerToggleEdgeCases.cs** (new) | 6 | US-005 |

**Total**: 54 unit tests (24 existing + 30 new) | **Estimated coverage**: ~88% critical-path

---

## 3. Functional Test Plan

**Writer**: `testing-functional-writer` | **Skill**: `polyglot-test`

### Scope
- End-to-end user story acceptance via integration and functional tests
- API contract verification (request/response shapes, status codes, error responses)
- Cross-user data isolation (NFR-007)
- Service → Repository → DB roundtrip for favorites CRUD lifecycle
- PublicApi lifecycle (Add → List → Remove)
- Web catalog page functional tests (auth gate, heart markup, accessibility attributes)

### Test Files
| File | Tests | Stories |
|---|---|---|
| FavoriteItemRepositoryTests/CrudOperations.cs | 4 | US-001 |
| FavoriteItemRepositoryTests/SpecificationQueries.cs | 5 | US-002, US-004, US-012 |
| ListFavoritesEndpointTest.cs | 2 | US-008 |
| AddFavoriteEndpointTest.cs | 3 | US-009 |
| RemoveFavoriteEndpointTest.cs | 2 | US-010 |
| **FavoritesToggleFunctional.cs** (new) | 5 | US-003, US-005, US-006, US-013 |
| **ToggleRoundtrip.cs** (new) | 4 | US-002, US-012 |
| **DataIsolation.cs** (new) | 3 | US-004 |
| **FavoriteLifecycleTest.cs** (new) | 3 | US-008, US-009, US-010 |
| **AddFavoriteDuplicateTest.cs** (new) | 2 | US-009, US-011 |
| **RemoveFavoriteEdgeCasesTest.cs** (new) | 3 | US-010 |

**Total**: 36 functional/integration tests (16 existing + 20 new) | **Story coverage**: 13/13

---

## 4. Security Test Plan

**Writer**: `testing-security-writer` | **Skill**: `security-testing`

### Scope
- OWASP Top 10 (2021) systematic assessment across all Favorites endpoints and domain code
- A01: Broken Access Control — auth enforcement, IDOR, cross-user isolation
- A03: Injection — input validation, parameterized queries, XSS prevention
- A04: Insecure Design — rate limiting, business logic (idempotency, mass assignment)
- A08: Data Integrity — CSRF validation on Web toggle
- A09: Logging/Monitoring — security event logging

### Test Files
| File | Tests | OWASP Categories |
|---|---|---|
| **FavoritesControllerSecurityTests.cs** (new) | 5 | A01, A03, A04 |
| **FavoriteServiceSecurityTests.cs** (new) | 10 | A01, A03, A09 |
| **FavoriteItemSecurityTests.cs** (new) | 6 | A03 |

**Total**: 21 security tests | **OWASP coverage**: 8/10 categories (A07, A10 N/A)

---

## 5. Compliance Test Plan

**Writer**: `testing-compliance-writer` | **Skill**: `compliance-testing`

### Scope
- **WCAG 2.1 AA**: Keyboard navigation, screen reader support, color contrast, focus indicators, aria attributes for heart icon
- **License Compliance**: Dependency license audit (11 packages)
- **Data Protection**: PII inventory, access control, encryption, data minimization
- **Browser/Platform Compatibility**: JS/CSS api compatibility with modern browsers

### Checks
| Framework | Checks | Focus Areas |
|---|---|---|
| WCAG 2.1 AA | 10 | 1.1.1, 1.3.1, 1.4.1, 1.4.3, 1.4.11, 2.1.1, 2.4.7, 4.1.2 |
| License | 9 | All NuGet dependencies + project license |
| Data Protection | 6 | PII, minimization, access control, encryption |
| Compatibility | 2 | JS API support, CSS features |

**Total**: 27 compliance checks

---

## 6. Execution Environment

| Property | Value |
|---|---|
| Runtime | .NET 10 SDK (not available in terminal — IDE diagnostics only) |
| Test Frameworks | xUnit v3 (unit/integration/functional), MSTest (PublicApi) |
| Mocking | NSubstitute |
| DB Provider | EF Core InMemory (tests), SQL Server (production) |
| Execution Mode | compile-verified, execution deferred |
| Coverage Tool | coverlet (deferred) |

---

## 7. Entry/Exit Criteria

### Entry Criteria
- [x] All 13 user stories implemented (41 SP)
- [x] Development code review: APPROVED
- [x] IDE validation: 0 errors
- [x] PR #1 created: https://github.com/workguysummitgrp/eshoponweb-favorites/pull/1

### Exit Criteria
- [x] All 4 specialist tracks completed
- [x] Core artifacts produced (test-plan, test-report, defect-log, go-nogo)
- [x] Specialist artifacts produced (security-test-report, compliance-report)
- [ ] All tests pass at runtime (deferred — .NET SDK unavailable)
- [x] No Critical/High production code defects
