# Weather Dashboard POC - Specification
# Stack: Angular 14 + .NET 8 Web API + Azure Deploy
# Date: March 13, 2026

---

## 1. User Story

**As a user I want to view real time weather data for my city so that I can plan my day.**

### Acceptance Criteria
- [ ] User can search for weather data by city name
- [ ] Weather data displays current conditions (temperature, humidity, wind speed, weather description)
- [ ] Weather data shows today's forecast with hourly breakdown
- [ ] Weather data shows 5-day extended forecast
- [ ] Data refreshes automatically every 15 minutes
- [ ] User can manually refresh weather data
- [ ] Application handles invalid city names gracefully
- [ ] Application works on desktop and mobile devices
- [ ] Weather data loads within 3 seconds of city selection

---

## 2. Functional Requirements

### 2.1 Core Features

#### Weather Search
- **Input**: City name search field with autocomplete
- **Validation**: Minimum 2 characters, maximum 50 characters
- **Behavior**: Search triggers on Enter key or Search button click
- **Error Handling**: Display user-friendly message for invalid cities

#### Current Weather Display
- **Temperature**: Current temperature in Celsius and Fahrenheit
- **Conditions**: Weather description (e.g., "Sunny", "Partly Cloudy") 
- **Details**: Humidity percentage, wind speed (km/h), pressure (hPa)
- **Visual**: Weather icon representing current conditions
- **Location**: Display city name, state/province, country
- **Timestamp**: Last updated time in local timezone

#### Today's Forecast
- **Timeline**: Hourly forecast for next 24 hours
- **Data Points**: Temperature, precipitation chance, wind speed per hour
- **Display**: Horizontal scroll or grid layout
- **Interaction**: Hover/tap to see detailed info for specific hour

#### Extended Forecast
- **Duration**: 5-day forecast starting from tomorrow
- **Daily Info**: High/low temperature, weather condition, precipitation chance
- **Layout**: Vertical list or card-based grid
- **Icons**: Weather condition icons for each day

### 2.2 Additional Features

#### Auto-Refresh
- **Interval**: Refresh data every 15 minutes automatically
- **Indicator**: Show last refresh timestamp
- **User Control**: Option to disable auto-refresh in settings

#### Manual Refresh
- **Trigger**: Refresh button in header/toolbar
- **Visual**: Loading spinner during refresh
- **Feedback**: Animation or notification when refresh completes

#### Responsive Design
- **Mobile**: Stack components vertically, touch-friendly buttons
- **Desktop**: Side-by-side layout, hover interactions
- **Breakpoints**: Bootstrap/Angular Flex Layout responsive breakpoints

---

## 3. Technical Requirements

### 3.1 Frontend (Angular 14)
```typescript
// Key Components Structure
SearchComponent          → City search input with autocomplete
CurrentWeatherComponent  → Display current weather conditions  
TodayForecastComponent  → Hourly forecast for today
ExtendedForecastComponent → 5-day forecast
WeatherCardComponent    → Reusable component for weather info display
LoadingSpinnerComponent → Reusable loading indicator

// Key Services
WeatherService          → HTTP calls to backend API
LocationService         → Handle geolocation and city search
CacheService           → Cache weather data for offline/performance
NotificationService    → User feedback (success/error messages)
```

### 3.2 Backend (.NET 8 Web API)
```csharp
// Controllers
WeatherController       → GET /api/weather/current/{cityName}
                       → GET /api/weather/forecast/{cityName}
                       → GET /api/weather/search/{query} (city autocomplete)

// Services  
IWeatherService        → Business logic for weather operations
IWeatherApiClient      → HTTP client wrapper for external weather API
ICacheService          → Redis/MemoryCache for API response caching
ILocationService       → City name validation and geocoding

// Models
WeatherResponse        → Current weather data structure
ForecastResponse       → Forecast data structure  
LocationSearchResult   → City search autocomplete results
ApiErrorResponse       → Standardized error response format
```

### 3.3 External API Integration
- **Provider**: OpenWeatherMap API (or similar reliable weather service)
- **API Key**: Store in Azure Key Vault, never hardcoded
- **Rate Limiting**: Respect API limits, implement request throttling
- **Caching**: Cache responses for 10-15 minutes to reduce API calls
- **Fallback**: Graceful degradation if external API is unavailable

---

## 4. API Specifications

### 4.1 Get Current Weather
```http
GET /api/weather/current/{cityName}

Response 200:
{
  "success": true,
  "message": "Weather data retrieved successfully",
  "data": {
    "location": {
      "city": "Toronto",
      "state": "Ontario", 
      "country": "Canada",
      "coordinates": { "lat": 43.70, "lon": -79.42 }
    },
    "current": {
      "temperature": { "celsius": 22, "fahrenheit": 72 },
      "feelsLike": { "celsius": 25, "fahrenheit": 77 },
      "humidity": 65,
      "windSpeed": 12,
      "pressure": 1013,
      "description": "Partly Cloudy",
      "icon": "partly-cloudy",
      "timestamp": "2026-03-13T14:30:00Z"
    }
  }
}

Response 404:
{
  "success": false,
  "message": "City not found. Please check the spelling and try again.",
  "data": null
}
```

### 4.2 Get Weather Forecast
```http
GET /api/weather/forecast/{cityName}

Response 200:
{
  "success": true,
  "message": "Forecast data retrieved successfully", 
  "data": {
    "location": { /* same as current weather */ },
    "todayHourly": [
      {
        "time": "2026-03-13T15:00:00Z",
        "temperature": { "celsius": 23, "fahrenheit": 73 },
        "precipitationChance": 10,
        "windSpeed": 15,
        "description": "Sunny"
      }
    ],
    "extendedDaily": [
      {
        "date": "2026-03-14",
        "high": { "celsius": 26, "fahrenheit": 79 },
        "low": { "celsius": 18, "fahrenheit": 64 },
        "precipitationChance": 20,
        "description": "Mostly Sunny",
        "icon": "mostly-sunny"
      }
    ]
  }
}
```

### 4.3 Search Cities (Autocomplete)
```http
GET /api/weather/search/{query}

Response 200:
{
  "success": true,
  "message": "City search results",
  "data": [
    {
      "displayName": "Toronto, Ontario, Canada",
      "cityName": "Toronto",
      "state": "Ontario",
      "country": "Canada", 
      "coordinates": { "lat": 43.70, "lon": -79.42 }
    }
  ]
}
```

---

## 5. User Interface Requirements

### 5.1 Layout Design
```
┌─────────────────────────────────────────────────────────────┐
│                    Weather Dashboard                        │
├─────────────────────────────────────────────────────────────┤
│  [🔍 Search for a city...] [Search] [🔄 Refresh]           │
├─────────────────────────────────────────────────────────────┤
│ ┌─────────────────┐  ┌─────────────────────────────────────┐ │
│ │   Current       │  │         Today's Forecast           │ │
│ │   Weather       │  │  [12PM] [1PM] [2PM] [3PM] [4PM]    │ │
│ │   Toronto, ON   │  │   22°   24°   26°   25°   23°      │ │
│ │      24°C       │  └─────────────────────────────────────┘ │
│ │   ☀️ Sunny      │                                         │
│ └─────────────────┘                                         │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │              5-Day Extended Forecast                    │ │
│ │  Thu  Fri  Sat  Sun  Mon                               │ │
│ │  26/18 28/20 24/16 22/14 25/17                         │ │
│ │   ☀️   ⛅   🌧️   ☁️   ☀️                              │ │
│ └─────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────┤
│ Last updated: 2:30 PM | Auto-refresh: ON                   │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 Responsive Breakpoints
- **Mobile (< 768px)**: Stack all components vertically
- **Tablet (768px - 1024px)**: Current weather left, forecast right  
- **Desktop (> 1024px)**: Full horizontal layout as shown above

### 5.3 Color Scheme & Styling
- **Primary**: Blue (#2196F3) for search button, links
- **Background**: Light gray (#F5F5F5) for overall page
- **Cards**: White (#FFFFFF) with subtle drop shadow
- **Text**: Dark gray (#333333) for readability
- **Accent**: Orange (#FF9800) for temperature highlights
- **Error**: Red (#F44336) for error messages
- **Success**: Green (#4CAF50) for success notifications

---

## 6. Performance Requirements

### 6.1 Response Time
- **API Response**: < 2 seconds for weather data retrieval
- **Page Load**: < 3 seconds for initial application load
- **Search Autocomplete**: < 500ms for city suggestions
- **Manual Refresh**: < 1 second to initiate, < 3 seconds to complete

### 6.2 Caching Strategy  
- **Frontend**: Cache last searched city in localStorage
- **Backend**: Cache API responses for 10 minutes using Redis/MemoryCache
- **CDN**: Static assets cached with appropriate headers

### 6.3 Optimization
- **Bundle Size**: Angular app bundle < 2MB compressed
- **Image Optimization**: Weather icons as SVG or WebP format
- **Lazy Loading**: Load forecast components only when needed
- **API Efficiency**: Batch requests where possible, avoid redundant calls

---

## 7. Azure Deployment Configuration

### 7.1 Azure Resources
```yaml
Resource Group: rg-weather-dashboard-poc
├── Azure Static Web Apps: swa-weather-frontend
│   ├── Custom Domain: weather-dashboard.yourdomain.com  
│   └── SSL Certificate: Auto-managed by Azure
├── Azure App Service: app-weather-api
│   ├── Runtime: .NET 8
│   ├── Tier: Basic B1 (can scale down to Free F1 for POC)
│   └── Custom Domain: api-weather.yourdomain.com
├── Azure Key Vault: kv-weather-secrets
│   └── Secrets: OpenWeatherMapApiKey, ConnectionStrings
└── Azure Application Insights: ai-weather-monitoring
    └── Monitoring: Frontend + Backend performance
```

### 7.2 Environment Configuration
```json
// Frontend (environment.prod.ts)
{
  "production": true,
  "apiUrl": "https://api-weather.yourdomain.com/api",
  "applicationInsights": {
    "instrumentationKey": "your-ai-key"
  }
}

// Backend (appsettings.Production.json) 
{
  "WeatherApi": {
    "BaseUrl": "https://api.openweathermap.org/data/2.5",
    "ApiKey": "#{OpenWeatherMapApiKey}#"  // from Key Vault
  },
  "Cors": {
    "AllowedOrigins": ["https://weather-dashboard.yourdomain.com"]
  },
  "ApplicationInsights": {
    "InstrumentationKey": "#{ApplicationInsightsKey}#"
  }
}
```

### 7.3 CI/CD Pipeline (GitHub Actions)
```yaml
# .github/workflows/main.yml
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  frontend-build-test:
    # Build Angular app, run tests, lint
    
  backend-build-test:  
    # Build .NET API, run unit tests, check coverage
    
  deploy-frontend:
    # Deploy to Azure Static Web Apps
    needs: frontend-build-test
    if: github.ref == 'refs/heads/main'
    
  deploy-backend:
    # Deploy to Azure App Service  
    needs: backend-build-test
    if: github.ref == 'refs/heads/main'
```

---

## 8. Security & Compliance

### 8.1 Data Protection
- **API Keys**: Stored in Azure Key Vault, rotated regularly
- **HTTPS Only**: All communication encrypted in transit
- **CORS Policy**: Restrict to specific frontend domain only
- **Input Validation**: Sanitize all user inputs on both frontend and backend

### 8.2 Error Handling
- **User-Friendly Messages**: No technical error details exposed to users
- **Logging**: Comprehensive error logging to Application Insights
- **Graceful Degradation**: App remains functional if external API fails
- **Rate Limiting**: Protect backend from abuse/excessive requests

---

## 9. Testing Strategy

### 9.1 Frontend Testing (Angular)
```typescript
// Unit Tests (70%+ coverage)
WeatherService.spec.ts          →  HTTP calls, error handling
SearchComponent.spec.ts         →  User interactions, validation  
CurrentWeatherComponent.spec.ts →  Data display, formatting
WeatherCardComponent.spec.ts    →  Reusable component behavior

// Integration Tests
WeatherModule.spec.ts          →  Component interactions
App.component.spec.ts          →  Full application flow

// E2E Tests (Cypress/Protractor)
weather-search.e2e.ts          →  Search and display weather
responsive-design.e2e.ts       →  Mobile and desktop layouts
error-scenarios.e2e.ts         →  Invalid cities, API failures
```

### 9.2 Backend Testing (.NET 8)
```csharp
// Unit Tests (70%+ coverage)  
WeatherController.Tests.cs      // API endpoints, response formatting
WeatherService.Tests.cs         // Business logic, external API calls
LocationService.Tests.cs        // City validation, geocoding

// Integration Tests
WeatherApi.Integration.Tests.cs // End-to-end API testing with TestServer

// Performance Tests 
WeatherApi.Load.Tests.cs        // Load testing with NBomber or similar
```

---

## 10. Success Metrics

### 10.1 Technical Metrics
- **Uptime**: 99.5% availability 
- **Performance**: 95% of requests complete within target response times
- **Error Rate**: < 1% of API calls result in unhandled errors
- **Test Coverage**: Maintain 70%+ code coverage across frontend and backend

### 10.2 User Experience Metrics
- **Load Time**: Average page load under 3 seconds
- **Search Success Rate**: 95% of valid city searches return results
- **Mobile Usability**: App functions correctly on iOS/Android browsers
- **Accessibility**: Meets WCAG 2.1 AA standards for screen readers

---

## 11. Future Enhancements (Out of Scope for POC)

- **User Accounts**: Save favorite cities, personal preferences
- **Weather Alerts**: Push notifications for severe weather
- **Maps Integration**: Interactive weather maps with radar
- **Historical Data**: View past weather trends and comparisons
- **Multi-Language**: Support for multiple languages and units
- **Offline Mode**: Basic functionality when internet connection is poor

---

**Document Version**: 1.0  
**Last Updated**: March 13, 2026  
**Next Review**: After POC completion