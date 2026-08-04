import {
  trigger,
  transition,
  style,
  animate,
  query,
  group
} from '@angular/animations';

export const pageSlideAnimation = trigger('pageSlideAnimation', [
  transition('* => *', [
    query(':enter, :leave', [
      style({
        position: 'absolute',
        left: 0,
        top: 0,
        width: '100%'
      })
    ], { optional: true }),
    query(':enter', [
      style({
        opacity: 0
      })
    ], { optional: true }),
    group([
      query(':leave', [
        animate(
          '250ms ease-out',
          style({
            opacity: 0
          })
        )
      ], { optional: true }),
      query(':enter', [
        animate(
          '250ms ease-out',
          style({
            opacity: 1
          })
        )
      ], { optional: true })
    ])
  ])
]);