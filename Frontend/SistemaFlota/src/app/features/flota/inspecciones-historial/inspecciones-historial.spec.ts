import {
  ComponentFixture,
  TestBed
}
from '@angular/core/testing';

import {
  InspeccionesHistorialComponent
}
from './inspecciones-historial';

describe(
  'InspeccionesHistorialComponent',
  () => {

    let component:
      InspeccionesHistorialComponent;

    let fixture:
      ComponentFixture<
        InspeccionesHistorialComponent
      >;

    beforeEach(
      async () => {

        await TestBed
          .configureTestingModule({

            imports: [
              InspeccionesHistorialComponent
            ]

          })
          .compileComponents();

        fixture =
          TestBed.createComponent(
            InspeccionesHistorialComponent
          );

        component =
          fixture.componentInstance;

        fixture.detectChanges();

      }
    );

    it(
      'should create',
      () => {

        expect(component)
          .toBeTruthy();

      }
    );

  }
);