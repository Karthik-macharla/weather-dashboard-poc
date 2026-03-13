/** Geographic coordinates */
export interface Coordinates {
  lat: number;
  lon: number;
}

/** Location information for a city */
export interface LocationInfo {
  city: string;
  state: string;
  country: string;
  coordinates: Coordinates;
}

/** Temperature in Celsius and Fahrenheit */
export interface Temperature {
  celsius: number;
  fahrenheit: number;
}

/** Current weather conditions */
export interface CurrentConditions {
  temperature: Temperature;
  feelsLike: Temperature;
  humidity: number;
  windSpeed: number;
  pressure: number;
  description: string;
  icon: string;
  timestamp: string;
}

/** Full current weather response data */
export interface WeatherResponse {
  location: LocationInfo;
  current: CurrentConditions;
}

/** Hourly forecast data point */
export interface HourlyForecast {
  time: string;
  temperature: Temperature;
  precipitationChance: number;
  windSpeed: number;
  description: string;
}

/** Daily forecast data point */
export interface DailyForecast {
  date: string;
  high: Temperature;
  low: Temperature;
  precipitationChance: number;
  description: string;
  icon: string;
}

/** Full forecast response data */
export interface ForecastResponse {
  location: LocationInfo;
  todayHourly: HourlyForecast[];
  extendedDaily: DailyForecast[];
}

/** City search autocomplete result */
export interface LocationSearchResult {
  displayName: string;
  cityName: string;
  state: string;
  country: string;
  coordinates: Coordinates;
}

/** Standardized API response wrapper */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}
