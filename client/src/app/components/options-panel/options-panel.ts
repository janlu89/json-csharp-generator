import { Component, input, output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GenerationOptions, DEFAULT_OPTIONS } from '../../services/conversion';

@Component({
  selector: 'app-options-panel',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './options-panel.html',
  styleUrl: './options-panel.scss'
})
export class OptionsPanel {
  direction = input<'json-to-csharp' | 'csharp-to-json'>('json-to-csharp');
  optionsChange = output<GenerationOptions>();
  autoConvertChange = output<boolean>();

  isOpen = signal(false);
  autoConvert = signal(false);

  options = signal<GenerationOptions>({ ...DEFAULT_OPTIONS });

  // Only show JSON→C# specific options when in that direction
  isJsonToCsharp = computed(() => this.direction() === 'json-to-csharp');

  togglePanel() {
    this.isOpen.set(!this.isOpen());
  }

  toggleAutoConvert() {
    const newValue = !this.autoConvert();
    this.autoConvert.set(newValue);
    this.autoConvertChange.emit(newValue);
  }

  updateOption<K extends keyof GenerationOptions>(
    key: K,
    value: GenerationOptions[K]
  ) {
    this.options.update(opts => ({ ...opts, [key]: value }));
    this.optionsChange.emit(this.options());
  }
}