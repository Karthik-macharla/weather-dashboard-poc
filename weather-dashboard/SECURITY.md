# Security Vulnerability Assessment — Angular 14 Frontend

## Overview

This document records known vulnerabilities in the Angular 14 dependencies used by this
project, their applicability to this application, and the mitigations in place.

> **Note:** `constitution.md` mandates Angular 14. All XSS vulnerabilities in the
> `<= 18.2.14` affected range have **no patch available** for Angular 14 from the
> Angular team. Upgrading to Angular 19+ would resolve all items below.

---

## CVE-1 — XSRF Token Leakage via Protocol-Relative URLs

| Field          | Detail |
|----------------|--------|
| Package        | `@angular/common` |
| Version        | 14.3.0 |
| Affected range | `< 19.2.16` |
| Patched in     | 19.2.16 (Angular 14: no patch) |

### Description
Angular HTTP Client leaks XSRF tokens when the configured API URL is a
protocol-relative URL (e.g., `//attacker.com/api`). The XSRF cookie value is
forwarded to the attacker-controlled host.

### Applicability — ✅ Not Exploitable
Two independent mitigations ensure this is not exploitable:

1. **No XSRF cookies in play**: `HttpClientXsrfModule.disable()` is explicitly
   configured in `WeatherModule`, disabling Angular's XSRF token reading entirely.
   There is no token to leak.
2. **GET-only API calls**: All three weather API endpoints (`/current`, `/forecast`,
   `/search`) are HTTP GET requests. Angular's XSRF interceptor never attaches tokens
   to GET requests even when XSRF is enabled.
3. **No protocol-relative URLs**: `environment.ts` uses `http://localhost:5000/api`
   and `environment.prod.ts` uses `/api`. A runtime guard in `WeatherService`
   constructor throws an error if a protocol-relative URL is ever configured.

### Code Locations
- `src/environments/environment.ts` — API URL (absolute, not protocol-relative)
- `src/environments/environment.prod.ts` — API URL (root-relative `/api`)
- `src/app/features/weather/weather.module.ts` — `HttpClientXsrfModule.disable()`
- `src/app/features/weather/services/weather.service.ts` — runtime URL guard

---

## CVE-2 — XSS via Unsanitized SVG Script Attributes

| Field          | Detail |
|----------------|--------|
| Package        | `@angular/compiler`, `@angular/core` |
| Version        | 14.3.0 |
| Affected range | `<= 18.2.14` |
| Patched in     | 19.2.18 / 20.3.16 / 21.0.7 (Angular 14: **no patch available**) |

### Description
Angular compiler fails to sanitize `<script>` attributes inside SVG elements,
allowing stored XSS if attacker-controlled content reaches an Angular template.

### Applicability — ✅ Not Exploitable
- **No SVG in templates**: All component templates (`weather.component.html`,
  `weather-card.component.html`) use only standard HTML. There are no `<svg>` elements.
- **No `[innerHTML]` bindings**: No component binds user input to `innerHTML` or
  calls any `DomSanitizer.bypassSecurityTrust*` method.
- **Input goes through Reactive Forms only**: User input is restricted to a
  single text field (`cityName`) validated by `Validators.maxLength(50)`. The
  value is only ever passed as a URL path segment via `encodeURIComponent()`.
- **CSP defence-in-depth**: `index.html` includes a `Content-Security-Policy`
  `<meta>` tag with `script-src 'self'`, blocking inline script execution even
  if a future code change accidentally introduces an unsafe binding.

---

## CVE-3 — Stored XSS via SVG Animation, SVG URL and MathML Attributes

| Field          | Detail |
|----------------|--------|
| Package        | `@angular/compiler` |
| Version        | 14.3.0 |
| Affected range | `<= 18.2.14` |
| Patched in     | 19.2.17 / 20.3.15 / 21.0.2 (Angular 14: **no patch available**) |

### Description
Angular compiler does not sanitize SVG animation attributes (`animate*`), SVG
`href`/`xlink:href` URL attributes, or MathML attributes, enabling stored XSS.

### Applicability — ✅ Not Exploitable
- **No SVG or MathML usage**: No component template uses SVG animations, SVG URL
  attributes, or MathML elements.
- Same template-level and CSP mitigations as CVE-2 apply.

---

## CVE-4 — i18n Cross-Site Scripting

| Field          | Detail |
|----------------|--------|
| Package        | `@angular/core` |
| Version        | 14.3.0 |
| Affected range | `<= 18.2.14` |
| Patched in     | 19.2.19 / 20.3.17 / 21.1.6 (Angular 14: **no patch available**) |

### Description
Angular i18n (`$localize` tag function, `i18n` template attributes) incorrectly
handles translated strings containing HTML, enabling XSS in localised templates.

### Applicability — ✅ Not Exploitable
- **i18n is not used**: This application uses no `$localize`, no `i18n` template
  attributes, and no `@angular/localize` package.

---

## Summary Table

| CVE Description              | Patch for v14 | Exploitable in this app | Mitigations applied |
|------------------------------|:-------------:|:-----------------------:|---------------------|
| XSRF Token Leakage           | ❌ None       | ❌ No                  | XSRF disabled; GET-only; URL guard |
| XSS via SVG Script Attrs     | ❌ None       | ❌ No                  | No SVG in templates; CSP |
| XSS via SVG Animation/MathML | ❌ None       | ❌ No                  | No SVG/MathML; CSP |
| i18n XSS                     | ❌ None       | ❌ No                  | i18n not used |

**None of the reported vulnerabilities are exploitable in this application** given
the current code patterns and the defence-in-depth mitigations applied.

For a production deployment beyond POC, upgrading to Angular 19+ is strongly
recommended to obtain vendor-patched versions of all packages listed above.
