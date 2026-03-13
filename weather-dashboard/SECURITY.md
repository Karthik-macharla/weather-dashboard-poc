# Security Vulnerability Assessment — Angular 14 Frontend

## Overview

This document records every advisory flagged against the Angular 14 packages used by
this project, a **semver-precise** analysis of which ranges actually include version
14.3.0, and the mitigations applied.

> **Methodology**: Each advisory entry carries one or more `Affected versions` ranges.
> We evaluate every range with `semver.satisfies('14.3.0', range)` to determine
> whether Angular 14.3.0 is genuinely in scope before treating an advisory as
> actionable. The advisory tool reports all known ranges for a CVE across all Angular
> major versions, so many entries do not apply to the installed version.

---

## CVE-1 — XSRF Token Leakage via Protocol-Relative URLs

| Field | Detail |
|-------|--------|
| Package | `@angular/common` |
| Installed | 14.3.0 |
| Applicable range | `< 19.2.16` → `semver.satisfies('14.3.0', '<19.2.16')` = **true** |
| Non-applicable range | `>= 20.0.0-next.0, < 20.3.14` → false (min is 20.x) |
| Non-applicable range | `>= 21.0.0-next.0, < 21.0.1` → false (min is 21.x) |
| Patched in | 19.2.16 (no backport for Angular 14) |

### Description
When Angular's HTTP client is configured to attach XSRF cookies and the API URL is
a protocol-relative URL (starting with `//`), the XSRF token is forwarded to the
attacker-controlled host.

### Status — ✅ Fully Mitigated

Three independent controls eliminate this vector:

1. **`HttpClientXsrfModule.disable()`** in `WeatherModule` — Angular reads no XSRF
   cookie and sends no XSRF header on any request, leaving nothing to leak.
2. **GET-only API surface** — All three weather API calls (`/current`, `/forecast`,
   `/search`) use HTTP GET. Angular's XSRF interceptor is a no-op for safe methods
   regardless of configuration.
3. **Runtime protocol-relative URL guard** — `WeatherService` constructor throws
   `Error('Protocol-relative API URLs are not allowed…')` if `environment.apiUrl`
   starts with `//`. Both environment files use safe URLs:
   `http://localhost:5000/api` (dev) and `/api` (prod).

Verified by Jasmine tests: `should not use a protocol-relative API URL` and
`should throw when constructed with a protocol-relative API URL`.

---

## CVE-2 — XSS via Unsanitized SVG Script Attributes

| Field | Detail |
|-------|--------|
| Packages | `@angular/compiler`, `@angular/core` |
| Installed | 14.3.0 |
| Applicable range | `<= 18.2.14` → `semver.satisfies('14.3.0', '<=18.2.14')` = **true** |
| Non-applicable range | `>= 19.0.0-next.0, < 19.2.18` → false (min is 19.x) |
| Non-applicable range | `>= 20.0.0-next.0, < 20.3.16` → false (min is 20.x) |
| Non-applicable range | `>= 21.0.0-next.0, < 21.0.7` → false (min is 21.x) |
| Non-applicable range | `>= 21.1.0-next.0, < 21.1.0-rc.0` → false (min is 21.1.x) |
| Patched in | For `<= 18.2.14`: **no patch available** from Angular team |

### Description
Angular compiler fails to sanitize `<script>` attributes inside `<svg>` elements,
allowing stored XSS if attacker-controlled SVG content reaches a template.

### Status — ✅ Not Exploitable + Defence-in-Depth Applied

- **No SVG in templates**: All component templates use only standard HTML. There are
  no `<svg>` elements anywhere in the application.
- **No unsafe HTML binding**: No component uses `[innerHTML]`, `[outerHTML]`, or
  any `DomSanitizer.bypassSecurityTrust*` method.
- **No user-controlled HTML rendering**: User input flows only through a Reactive
  Forms text field whose value is URL-encoded via `encodeURIComponent()`.
- **CSP defence-in-depth**: `index.html` carries
  `Content-Security-Policy: script-src 'self'` via `<meta http-equiv>`. This blocks
  inline script execution even if a future code change accidentally introduced an
  unsafe binding.

---

## CVE-3 — Stored XSS via SVG Animation, SVG URL and MathML Attributes

| Field | Detail |
|-------|--------|
| Package | `@angular/compiler` |
| Installed | 14.3.0 |
| Applicable range | `<= 18.2.14` → `semver.satisfies('14.3.0', '<=18.2.14')` = **true** |
| Non-applicable range | `>= 19.0.0-next.0, < 19.2.17` → false (min is 19.x) |
| Non-applicable range | `>= 20.0.0-next.0, < 20.3.15` → false (min is 20.x) |
| Non-applicable range | `>= 21.0.0-next.0, < 21.0.2` → false (min is 21.x) |
| Patched in | For `<= 18.2.14`: **no patch available** |

### Description
Angular compiler does not sanitize SVG animation attributes (`animate*`), SVG
`href`/`xlink:href`, or MathML attributes, enabling stored XSS.

### Status — ✅ Not Exploitable + Defence-in-Depth Applied

Same controls as CVE-2 apply: no SVG or MathML elements in any template, no
unsafe HTML binding, and the CSP header blocks inline scripts.

---

## CVE-4 — i18n Cross-Site Scripting

| Field | Detail |
|-------|--------|
| Package | `@angular/core` |
| Installed | 14.3.0 |
| Applicable range | `<= 18.2.14` → `semver.satisfies('14.3.0', '<=18.2.14')` = **true** |
| Non-applicable range | `>= 19.0.0-next.0, <= 19.2.18` → false (min is 19.x) |
| Non-applicable range | `>= 20.0.0-next.0, <= 20.3.16` → false (min is 20.x) |
| Non-applicable range | `>= 21.0.0-next.0, <= 21.1.5` → false (min is 21.x) |
| Non-applicable range | `>= 21.2.0-next.0, <= 21.2.0-rc.0` → false (min is 21.2.x) |
| Patched in | For `<= 18.2.14`: **no patch available** |

> Note: The `<=` (inclusive) upper bound in the non-applicable ranges above matches
> the advisory data exactly — i18n XSS advisories use inclusive upper bounds, unlike
> CVE-1/2/3 which use exclusive `<` bounds. The difference reflects the individual
> advisory definitions, not an error in this document.

### Description
Angular i18n (`$localize` tag function, `i18n` template attributes) mishandles
translated strings containing HTML markup, enabling XSS in localised templates.

### Status — ✅ Not Exploitable

- **i18n is not used**: No `$localize` call, no `i18n` attribute, and no
  `@angular/localize` dependency exists anywhere in the project.

---

## Summary

| CVE | v14.3.0 actually affected? | Patch for v14 | Action taken |
|-----|:--------------------------:|:-------------:|--------------|
| XSRF Token Leakage | **Yes** (`< 19.2.16` is satisfied) | ❌ None | XSRF disabled; GET-only API; runtime URL guard |
| XSS via SVG Script Attrs | **Yes** (`<= 18.2.14` is satisfied) | ❌ None | No SVG in templates; CSP `script-src 'self'` |
| XSS via SVG Animation/MathML | **Yes** (`<= 18.2.14` is satisfied) | ❌ None | No SVG/MathML in templates; CSP |
| i18n XSS | **Yes** (`<= 18.2.14` is satisfied) | ❌ None | i18n not used |

> **On the `>= 19.x` / `>= 20.x` / `>= 21.x` ranges**: Every advisory for CVE-2,
> CVE-3, and CVE-4 includes both a `<= 18.2.14` sub-range (which includes 14.3.0)
> and separate sub-ranges for Angular 19/20/21 (which do not include 14.3.0).
> Advisory scanners that report a package/CVE pair without filtering by the specific
> installed version will surface all sub-ranges for the same CVE, including ones that
> do not apply to version 14.3.0. The semver column above is the authoritative
> determination of applicability.

For a production system, upgrading to Angular 19+ is strongly recommended to obtain
vendor-patched releases of all packages listed above.
