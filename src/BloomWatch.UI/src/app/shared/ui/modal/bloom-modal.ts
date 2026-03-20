import {
  Component,
  input,
  output,
  effect,
  ElementRef,
  viewChild,
  OnDestroy,
} from '@angular/core';

@Component({
  selector: 'bloom-modal',
  standalone: true,
  styleUrl: './bloom-modal.scss',
  template: `
    @if (open()) {
      <div
        class="bloom-modal__backdrop"
        (click)="onBackdropClick()"
        (keydown)="onKeydown($event)"
      >
        <div
          class="bloom-modal__dialog"
          [style.max-width]="width()"
          role="dialog"
          aria-modal="true"
          (click)="$event.stopPropagation()"
          #dialog
        >
          <div class="bloom-modal__header">
            <ng-content select="[bloomModalHeader]" />
            <button
              class="bloom-modal__close"
              type="button"
              aria-label="Close modal"
              (click)="close()"
            >
              &times;
            </button>
          </div>
          <div class="bloom-modal__body">
            <ng-content />
          </div>
          <div class="bloom-modal__footer">
            <ng-content select="[bloomModalFooter]" />
          </div>
        </div>
      </div>
    }
  `,
})
export class BloomModalComponent implements OnDestroy {
  readonly open = input<boolean>(false);
  readonly width = input<string>('32rem');

  readonly closed = output<void>();

  readonly dialogRef = viewChild<ElementRef<HTMLElement>>('dialog');

  private previouslyFocusedElement: HTMLElement | null = null;
  private previousOverflow = '';

  constructor() {
    effect(() => {
      if (this.open()) {
        this.onOpen();
      } else {
        this.onClose();
      }
    });
  }

  ngOnDestroy(): void {
    this.restoreBodyScroll();
  }

  close(): void {
    this.closed.emit();
  }

  onBackdropClick(): void {
    this.close();
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      event.preventDefault();
      this.close();
      return;
    }

    if (event.key === 'Tab') {
      this.trapFocus(event);
    }
  }

  private onOpen(): void {
    this.previouslyFocusedElement = document.activeElement as HTMLElement;
    this.previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';

    // Focus first focusable element after render
    setTimeout(() => {
      const dialog = this.dialogRef()?.nativeElement;
      if (!dialog) return;
      const focusable = this.getFocusableElements(dialog);
      if (focusable.length > 0) {
        focusable[0].focus();
      }
    });
  }

  private onClose(): void {
    this.restoreBodyScroll();
    if (this.previouslyFocusedElement) {
      this.previouslyFocusedElement.focus();
      this.previouslyFocusedElement = null;
    }
  }

  private restoreBodyScroll(): void {
    document.body.style.overflow = this.previousOverflow;
  }

  private trapFocus(event: KeyboardEvent): void {
    const dialog = this.dialogRef()?.nativeElement;
    if (!dialog) return;

    const focusable = this.getFocusableElements(dialog);
    if (focusable.length === 0) return;

    const first = focusable[0];
    const last = focusable[focusable.length - 1];

    if (event.shiftKey) {
      if (document.activeElement === first) {
        event.preventDefault();
        last.focus();
      }
    } else {
      if (document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    }
  }

  private getFocusableElements(container: HTMLElement): HTMLElement[] {
    const selector =
      'a[href], button:not([disabled]), input:not([disabled]), textarea:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])';
    return Array.from(container.querySelectorAll<HTMLElement>(selector));
  }
}
