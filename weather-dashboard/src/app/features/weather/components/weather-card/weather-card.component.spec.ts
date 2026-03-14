import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WeatherCardComponent } from './weather-card.component';
import { WeatherResponse } from '../../models/weather.model';
import { CommonModule } from '@angular/common';

const mockWeather: WeatherResponse = {
  location: {
    city: 'Toronto',
    state: 'Ontario',
    country: 'Canada',
    coordinates: { lat: 43.70, lon: -79.42 }
  },
  current: {
    temperature: { celsius: 22, fahrenheit: 72 },
    feelsLike: { celsius: 25, fahrenheit: 77 },
    humidity: 65,
    windSpeed: 12,
    pressure: 1013,
    description: 'Partly Cloudy',
    icon: 'partly-cloudy',
    timestamp: '2026-03-13T14:30:00Z'
  }
};

describe('WeatherCardComponent', () => {
  let component: WeatherCardComponent;
  let fixture: ComponentFixture<WeatherCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [WeatherCardComponent],
      imports: [CommonModule]
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherCardComponent);
    component = fixture.componentInstance;
    component.weather = mockWeather;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the city name', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.weather-card__city')?.textContent).toContain('Toronto');
  });

  it('should display state and country', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.weather-card__region')?.textContent).toContain('Ontario');
    expect(el.querySelector('.weather-card__region')?.textContent).toContain('Canada');
  });

  it('should display the temperature in Celsius', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.weather-card__temperature')?.textContent).toContain('22');
  });

  it('should display the weather description', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.weather-card__description')?.textContent).toContain('Partly Cloudy');
  });

  it('should display humidity', () => {
    const el: HTMLElement = fixture.nativeElement;
    const detailValues = el.querySelectorAll('.weather-card__detail-value');
    const texts = Array.from(detailValues).map(v => v.textContent ?? '');
    expect(texts.some(t => t.includes('65%'))).toBeTrue();
  });

  it('should display wind speed', () => {
    const el: HTMLElement = fixture.nativeElement;
    const detailValues = el.querySelectorAll('.weather-card__detail-value');
    const texts = Array.from(detailValues).map(v => v.textContent ?? '');
    expect(texts.some(t => t.includes('12'))).toBeTrue();
  });

  it('should display pressure', () => {
    const el: HTMLElement = fixture.nativeElement;
    const detailValues = el.querySelectorAll('.weather-card__detail-value');
    const texts = Array.from(detailValues).map(v => v.textContent ?? '');
    expect(texts.some(t => t.includes('1013'))).toBeTrue();
  });

  it('should display feels like temperature', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.weather-card__feels-like')?.textContent).toContain('25');
  });

  it('should update view when weather input changes', () => {
    const updatedWeather: WeatherResponse = {
      ...mockWeather,
      location: { ...mockWeather.location, city: 'Vancouver' },
      current: { ...mockWeather.current, temperature: { celsius: 10, fahrenheit: 50 } }
    };
    component.weather = updatedWeather;
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.weather-card__city')?.textContent).toContain('Vancouver');
    expect(el.querySelector('.weather-card__temperature')?.textContent).toContain('10');
  });
});
