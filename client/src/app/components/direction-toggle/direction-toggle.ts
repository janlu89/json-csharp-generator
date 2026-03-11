import { Component, signal, output, input, OnChanges, SimpleChanges } from '@angular/core';

export type ConversionDirection = 'json-to-csharp' | 'csharp-to-json';

@Component({
  selector: 'app-direction-toggle',
  standalone: true,
  templateUrl: './direction-toggle.html',
  styleUrl: './direction-toggle.scss'
})
export class DirectionToggle implements OnChanges {
  externalDirection = input<ConversionDirection | null>(null);
  directionChange = output<ConversionDirection>();

  direction = signal<ConversionDirection>('json-to-csharp');

  ngOnChanges(changes: SimpleChanges) {
    if (changes['externalDirection']?.currentValue) {
      this.direction.set(changes['externalDirection'].currentValue);
    }
  }

  setDirection(dir: ConversionDirection) {
    this.direction.set(dir);
    this.directionChange.emit(dir);
  }
}