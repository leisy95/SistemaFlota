import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarTraslado } from './listar-traslado';

describe('ListarTraslado', () => {
  let component: ListarTraslado;
  let fixture: ComponentFixture<ListarTraslado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarTraslado]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarTraslado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
