import { Component, signal, output } from '@angular/core';

export type ConversionDirection = 'json-to-csharp' | 'csharp-to-json';

@Component({
  selector: 'app-direction-toggle',
  standalone: true,
  template: `
    <div class="direction-bar">
      <div class="toggle-group">
        <button
          class="toggle-btn"
          [class.active]="direction() === 'json-to-csharp'"
          (click)="setDirection('json-to-csharp')">
          JSON → C#
        </button>
        <button
          class="toggle-btn"
          [class.active]="direction() === 'csharp-to-json'"
          (click)="setDirection('csharp-to-json')">
          C# → JSON
        </button>
      </div>
    </div>
  `,
  styles: [`
    .direction-bar {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: var(--space-sm) var(--space-lg);
      background-color: var(--bg-secondary);
      border-bottom: 1px solid var(--border);
      flex-shrink: 0;
    }

    .toggle-group {
      display: flex;
      background-color: var(--bg-tertiary);
      border-radius: var(--border-radius);
      padding: 2px;
      gap: 2px;
    }

    .toggle-btn {
      background: none;
      border: none;
      color: var(--text-secondary);
      padding: var(--space-xs) var(--space-md);
      border-radius: var(--border-radius);
      cursor: pointer;
      font-family: var(--font-ui);
      font-size: var(--font-size-sm);
      font-weight: 500;
      transition: all var(--transition);

      &:hover {
        color: var(--text-primary);
      }

      &.active {
        background-color: var(--bg-elevated);
        color: var(--accent);
        box-shadow: var(--shadow);
      }
    }
  `]
})
export class DirectionToggle {
  direction = signal<ConversionDirection>('json-to-csharp');
  directionChange = output<ConversionDirection>();

  setDirection(dir: ConversionDirection) {
    this.direction.set(dir);
    this.directionChange.emit(dir);
  }
}