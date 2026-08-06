import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CorteMesInventario } from './corte-mes-inventario';

describe('CorteMesInventario', () => {
  let component: CorteMesInventario;
  let fixture: ComponentFixture<CorteMesInventario>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CorteMesInventario]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CorteMesInventario);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
