import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HttpClientXsrfModule } from '@angular/common/http';
import { WeatherComponent } from './components/weather/weather.component';
import { WeatherCardComponent } from './components/weather-card/weather-card.component';

/**
 * Feature module for all weather-related functionality.
 * Declares smart and dumb weather components.
 *
 * Security note: HttpClientXsrfModule is explicitly disabled because:
 * - All API calls in this module are HTTP GET (read-only), so Angular
 *   never attaches XSRF headers to them anyway.
 * - The backend API does not issue XSRF cookies.
 * This eliminates the XSRF token leakage vector reported in
 * @angular/common < 19.2.16 (no patch available for Angular 14).
 */
@NgModule({
  declarations: [
    WeatherComponent,
    WeatherCardComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    HttpClientXsrfModule.disable()
  ],
  exports: [
    WeatherComponent
  ]
})
export class WeatherModule {}
