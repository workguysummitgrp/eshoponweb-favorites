# Defect Log — eShopOnWeb Favorites Enhancement

| Field | Value |
|---|---|
| **Project** | eShopOnWeb Favorites |
| **Slug** | eshoponweb-favorites |
| **Epic** | DEMO3-1 |
| **Phase** | Testing |
| **Date** | 2026-03-31 |

---

## Summary

| Severity | Count | GO/NO-GO Impact |
|---|---|---|
| Critical | 0 | — |
| High | 1 | Test infrastructure only — no production impact |
| Medium | 3 | Hardening recommendations — non-blocking |
| Low | 5 | Minor improvements — non-blocking |
| **Total** | **9** | **GO** |

---

## Defects

### High Severity

| ID | Track | Component | Description | Remediation | Status |
|---|---|---|---|---|---|
| DEF-001 | functional | `ApiTokenHelper.cs` | `CreateToken()` omits `ClaimTypes.NameIdentifier` claim. PublicApi endpoints extract UserId from this claim. | Add `new Claim(ClaimTypes.NameIdentifier, userName)` to claims list. | Open |

### Medium Severity

| ID | Track | OWASP | Component | Description | Remediation | Status |
|---|---|---|---|---|---|---|
| SEC-F01 | security | A04 | FavoritesController | No rate limiting on toggle endpoint | Add sliding window rate limiting (30 req/min per user) | Open |
| SEC-F02 | security | A04 | ListFavoritesEndpoint | No pagination on GET /api/favorites | Add pageIndex/pageSize with maxPageSize=100 | Open |
| SEC-F03 | security | A04 | PublicApi endpoints | No rate limiting on POST/DELETE favorites | Add rate limiting (60 req/min per user) | Open |

### Low Severity

| ID | Track | Component | Description | Remediation | Status |
|---|---|---|---|---|---|
| SEC-F04 | security | FavoritesController | No validation failure logging | Add LogWarning on invalid CatalogItemId | Open |
| SEC-F05 | security | PublicApi endpoints | No validation failure logging | Add LogWarning on 400/409 responses | Open |
| CMP-DEF-001 | compliance | _variables.scss | Heart color #E91E63 ≈ 4.0:1 contrast (below 4.5:1 AA) | Change to #C2185B (≈ 5.4:1) | Open |
| CMP-DEF-002 | compliance | favorites.js | No aria-live region for toggle announcements | Add aria-live="polite" div | Open |
| CMP-DEF-003 | compliance | FavoriteService | Data deletion mechanism unconfirmed | Verify DeleteAllByUserId method | Open |
