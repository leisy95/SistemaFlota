import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarInventario } from './listar-inventario';

describe('ListarInventario', () => {
  let component: ListarInventario;
  let fixture: ComponentFixture<ListarInventario>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarInventario]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarInventario);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
