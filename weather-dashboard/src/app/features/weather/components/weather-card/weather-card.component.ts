import { Component, Input } from '@angular/core';
import { WeatherResponse } from '../../models/weather.model';

/**
 * Dumb component that displays current weather information for a city.
 * Receives data via @Input and emits no events.
 */
@Component({
  selector: 'app-weather-card',
  templateUrl: './weather-card.component.html',
  styleUrls: ['./weather-card.component.scss']
})
export class WeatherCardComponent {
  /** The weather data to display. */
  @Input() weather!: WeatherResponse;
}
