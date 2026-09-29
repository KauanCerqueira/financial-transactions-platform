import { Directive, ElementRef, effect, inject, input } from '@angular/core';

const formatter = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
const reducedMotionQuery = '(prefers-reduced-motion: reduce)';

@Directive({ selector: '[appCountUp]' })
export class CountUpDirective {
  readonly appCountUp = input.required<number>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private frame = 0;
  private current = 0;

  constructor() {
    effect(() => this.animateTo(this.appCountUp()));
  }

  private animateTo(target: number): void {
    cancelAnimationFrame(this.frame);

    const from = this.current;

    if (from === target || this.prefersReducedMotion()) {
      this.settle(target);
      return;
    }

    const duration = 600;
    const start = performance.now();

    const step = (now: number) => {
      const progress = Math.min((now - start) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);

      this.current = from + (target - from) * eased;
      this.render(this.current);

      if (progress < 1) {
        this.frame = requestAnimationFrame(step);
      } else {
        this.settle(target);
      }
    };

    this.frame = requestAnimationFrame(step);
  }

  private settle(target: number): void {
    this.current = target;
    this.render(target);
  }

  private prefersReducedMotion(): boolean {
    return (
      typeof window !== 'undefined' &&
      typeof window.matchMedia === 'function' &&
      window.matchMedia(reducedMotionQuery).matches
    );
  }

  private render(value: number): void {
    this.host.nativeElement.textContent = formatter.format(value);
  }
}
