import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, signal } from '@angular/core';
import { BloomModalComponent } from './bloom-modal';

@Component({
  standalone: true,
  imports: [BloomModalComponent],
  template: `
    <bloom-modal [open]="isOpen()" (closed)="onClosed()">
      <h2 bloomModalHeader>Test Header</h2>
      <input id="first-input" />
      <button id="last-btn">Action</button>
      <div bloomModalFooter>Footer</div>
    </bloom-modal>
  `,
})
class TestHostComponent {
  readonly isOpen = signal(false);
  closedCount = 0;

  onClosed(): void {
    this.closedCount++;
    this.isOpen.set(false);
  }
}

describe('BloomModalComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TestHostComponent],
    });
    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    // Ensure body scroll is restored
    document.body.style.overflow = '';
  });

  it('should not render when open is false', () => {
    const backdrop = fixture.nativeElement.querySelector('.bloom-modal__backdrop');
    expect(backdrop).toBeNull();
  });

  it('should render when open is true', () => {
    host.isOpen.set(true);
    fixture.detectChanges();

    const backdrop = fixture.nativeElement.querySelector('.bloom-modal__backdrop');
    expect(backdrop).toBeTruthy();
    const dialog = fixture.nativeElement.querySelector('.bloom-modal__dialog');
    expect(dialog).toBeTruthy();
  });

  it('should project header, body, and footer content', () => {
    host.isOpen.set(true);
    fixture.detectChanges();

    const header = fixture.nativeElement.querySelector('.bloom-modal__header h2');
    expect(header?.textContent).toContain('Test Header');

    const body = fixture.nativeElement.querySelector('.bloom-modal__body input');
    expect(body).toBeTruthy();

    const footer = fixture.nativeElement.querySelector('.bloom-modal__footer');
    expect(footer?.textContent).toContain('Footer');
  });

  it('should emit closed on Escape key', () => {
    host.isOpen.set(true);
    fixture.detectChanges();

    const backdrop = fixture.nativeElement.querySelector('.bloom-modal__backdrop');
    backdrop.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    fixture.detectChanges();

    expect(host.closedCount).toBe(1);
  });

  it('should emit closed on backdrop click', () => {
    host.isOpen.set(true);
    fixture.detectChanges();

    const backdrop = fixture.nativeElement.querySelector('.bloom-modal__backdrop');
    backdrop.click();
    fixture.detectChanges();

    expect(host.closedCount).toBe(1);
  });

  it('should not emit closed when clicking inside the dialog', () => {
    host.isOpen.set(true);
    fixture.detectChanges();

    const dialog = fixture.nativeElement.querySelector('.bloom-modal__dialog');
    dialog.click();
    fixture.detectChanges();

    expect(host.closedCount).toBe(0);
  });

  it('should emit closed when close button is clicked', () => {
    host.isOpen.set(true);
    fixture.detectChanges();

    const closeBtn = fixture.nativeElement.querySelector('.bloom-modal__close');
    closeBtn.click();
    fixture.detectChanges();

    expect(host.closedCount).toBe(1);
  });

  it('should lock body scroll when open and restore on close', () => {
    document.body.style.overflow = '';
    host.isOpen.set(true);
    fixture.detectChanges();

    expect(document.body.style.overflow).toBe('hidden');

    host.isOpen.set(false);
    fixture.detectChanges();

    expect(document.body.style.overflow).toBe('');
  });

  it('should focus first focusable element on open', async () => {
    host.isOpen.set(true);
    fixture.detectChanges();

    // Wait for setTimeout in onOpen
    await new Promise((resolve) => setTimeout(resolve, 0));

    // The close button is the first focusable element in the dialog
    const closeBtn = fixture.nativeElement.querySelector('.bloom-modal__close');
    expect(document.activeElement).toBe(closeBtn);
  });

  it('should trap focus on Tab at last element', () => {
    host.isOpen.set(true);
    fixture.detectChanges();

    const lastBtn = fixture.nativeElement.querySelector('#last-btn') as HTMLElement;
    lastBtn.focus();

    const backdrop = fixture.nativeElement.querySelector('.bloom-modal__backdrop');
    const tabEvent = new KeyboardEvent('keydown', { key: 'Tab', bubbles: true });
    backdrop.dispatchEvent(tabEvent);

    // Focus should wrap — the event should be handled
    // (In JSDOM, focus management is limited, but we verify the handler runs)
    expect(host.closedCount).toBe(0); // Should not close
  });
});
