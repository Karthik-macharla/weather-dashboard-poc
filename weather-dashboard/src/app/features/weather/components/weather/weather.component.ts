import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ForecastResponse, WeatherResponse } from '../../models/weather.model';
import { WeatherService } from '../../services/weather.service';

/** Minimum allowed length for a city search query. */
const MIN_CITY_LENGTH = 1;

/**
 * Smart container component for the weather dashboard.
 * Manages state, handles user interactions, and coordinates data loading.
 */
@Component({
  selector: 'app-weather',
  templateUrl: './weather.component.html',
  styleUrls: ['./weather.component.scss']
})
export class WeatherComponent implements OnInit, OnDestroy {
  /** Reactive form group for the city search input. */
  searchForm: FormGroup;

  /** Current weather data for the searched city. */
  weather: WeatherResponse | null = null;

  /** Forecast data for the searched city. */
  forecast: ForecastResponse | null = null;

  /** Whether a data fetch is in progress. */
  isLoading = false;

  /** Error message to display to the user. */
  errorMessage: string | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly weatherService: WeatherService
  ) {
    this.searchForm = this.fb.group({
      cityName: ['', [Validators.required, Validators.minLength(MIN_CITY_LENGTH), Validators.maxLength(50)]]
    });
  }

  /** @inheritdoc */
  ngOnInit(): void {}

  /** @inheritdoc */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Returns the cityName form control for use in the template.
   */
  get cityNameControl() {
    return this.searchForm.get('cityName');
  }

  /**
   * Handles the search form submission.
   * Validates input and initiates weather data retrieval.
   */
  onSearch(): void {
    if (this.searchForm.invalid) {
      this.searchForm.markAllAsTouched();
      return;
    }

    const city: string = this.cityNameControl?.value?.trim();
    if (!city) {
      return;
    }

    this.loadWeatherData(city);
  }

  /**
   * Loads current weather and forecast data for the given city.
   * @param city The city name to load data for.
   */
  private loadWeatherData(city: string): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.weather = null;
    this.forecast = null;

    this.weatherService
      .getCurrentWeather(city)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.weather = data;
          this.loadForecast(city);
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'City not found. Please check the spelling and try again.';
        }
      });
  }

  /**
   * Loads the forecast data for the given city.
   * @param city The city name to load forecast for.
   */
  private loadForecast(city: string): void {
    this.weatherService
      .getForecast(city)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.forecast = data;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'Unable to load forecast data. Please try again.';
        }
      });
  }
}
