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
        transform: 'translateX(-80%) scale(0.98)',
        opacity: 0,
        filter: 'blur(3px)'
      })
    ], { optional: true }),

    group([

      query(':leave', [
        animate(
          '500ms cubic-bezier(0.22, 1, 0.36, 1)',
          style({
            transform: 'translateX(80%) scale(0.98)',
            opacity: 0,
            filter: 'blur(3px)'
          })
        )
      ], { optional: true }),

      query(':enter', [
        animate(
          '500ms cubic-bezier(0.22, 1, 0.36, 1)',
          style({
            transform: 'translateX(0) scale(1)',
            opacity: 1,
            filter: 'blur(0)'
          })
        )
      ], { optional: true })

    ])

  ])

]);