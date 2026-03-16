import { Component, input, output, computed, signal, forwardRef } from '@angular/core';
import { NgClass } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export type BloomInputSize = 'sm' | 'md' | 'lg';

let bloomInputNextId = 0;

@Component({
  selector: 'bloom-input',
  standalone: true,
  imports: [NgClass],
  styleUrl: './bloom-input.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => BloomInputComponent),
      multi: true,
    },
  ],
  template: `
    <div [ngClass]="wrapperClasses()">
      <!-- Label -->
      @if (label()) {
        <label [for]="inputId()" class="bloom-input__label">
          {{ label() }}
          @if (required()) {
            <span class="bloom-input__required" aria-hidden="true">*</span>
          }
        </label>
      }

      <!-- Input wrapper with prefix/suffix slots -->
      <div class="bloom-input__field-wrapper" [class.bloom-input__field-wrapper--focused]="isFocused()">
        <!-- Prefix icon slot -->
        <span class="bloom-input__prefix">
          <ng-content select="[bloomInputPrefix]" />
        </span>

        <input
          class="bloom-input__field"
          [id]="inputId()"
          [type]="type()"
          [placeholder]="placeholder()"
          [disabled]="disabled()"
          [readonly]="readonly()"
          [required]="required()"
          [attr.aria-invalid]="!!error()"
          [attr.aria-describedby]="error() ? inputId() + '-error' : hint() ? inputId() + '-hint' : null"
          [attr.autocomplete]="autocomplete()"
          [value]="value()"
          (input)="onInput($event)"
          (focus)="onFocus()"
          (blur)="onBlur()"
        />

        <!-- Suffix icon slot -->
        <span class="bloom-input__suffix">
          <ng-content select="[bloomInputSuffix]" />
        </span>
      </div>

      <!-- Hint text -->
      @if (hint() && !error()) {
        <p class="bloom-input__hint" [id]="inputId() + '-hint'">{{ hint() }}</p>
      }

      <!-- Error message -->
      @if (error()) {
        <p class="bloom-input__error" [id]="inputId() + '-error'" role="alert">
          {{ error() }}
        </p>
      }
    </div>
  `,
})
export class BloomInputComponent implements ControlValueAccessor {
  readonly label = input<string>('');
  readonly placeholder = input<string>('');
  readonly type = input<string>('text');
  readonly size = input<BloomInputSize>('md');
  readonly disabled = input<boolean>(false);
  readonly readonly = input<boolean>(false);
  readonly required = input<boolean>(false);
  readonly error = input<string>('');
  readonly hint = input<string>('');
  readonly inputId = input<string>(`bloom-input-${bloomInputNextId++}`);
  readonly autocomplete = input<string>('off');

  readonly valueChange = output<string>();

  readonly value = signal('');
  readonly isFocused = signal(false);

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  readonly wrapperClasses = computed(() => {
    return {
      'bloom-input': true,
      [`bloom-input--${this.size()}`]: true,
      'bloom-input--error': !!this.error(),
      'bloom-input--disabled': this.disabled(),
      'bloom-input--focused': this.isFocused(),
    };
  });

  // ControlValueAccessor implementation
  writeValue(val: string): void {
    this.value.set(val ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    // Handled by input signal externally
  }

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.value.set(target.value);
    this.onChange(target.value);
    this.valueChange.emit(target.value);
  }

  onFocus(): void {
    this.isFocused.set(true);
  }

  onBlur(): void {
    this.isFocused.set(false);
    this.onTouched();
  }
}
