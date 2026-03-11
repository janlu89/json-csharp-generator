import { Component, OnInit, inject, signal } from '@angular/core';
import { Header } from './components/header/header';
import { DirectionToggle, ConversionDirection } from './components/direction-toggle/direction-toggle';
import { SplitPane } from './components/split-pane/split-pane';
import { ThemeService } from './services/theme';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [Header, DirectionToggle, SplitPane],
  template: `
    <div class="app-shell">
      <app-header />
      <app-direction-toggle (directionChange)="onDirectionChange($event)" />
      <app-split-pane
        class="split-pane-container"
        [direction]="direction()" />
    </div>
  `,
  styles: [`
    .app-shell {
      display: flex;
      flex-direction: column;
      height: 100vh;
      overflow: hidden;
      background-color: var(--bg-primary);
    }

    .split-pane-container {
      flex: 1;
      min-height: 0;
    }
  `]
})
export class App implements OnInit {
  private themeService = inject(ThemeService);
  direction = signal<ConversionDirection>('json-to-csharp');

  ngOnInit() {
    this.themeService.init();
  }

  onDirectionChange(dir: ConversionDirection) {
    this.direction.set(dir);
  }
}