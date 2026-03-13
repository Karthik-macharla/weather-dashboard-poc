import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { WeatherComponent } from './components/weather/weather.component';
import { WeatherCardComponent } from './components/weather-card/weather-card.component';

/**
 * Feature module for all weather-related functionality.
 * Declares smart and dumb weather components.
 */
@NgModule({
  declarations: [
    WeatherComponent,
    WeatherCardComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule
  ],
  exports: [
    WeatherComponent
  ]
})
export class WeatherModule {}
