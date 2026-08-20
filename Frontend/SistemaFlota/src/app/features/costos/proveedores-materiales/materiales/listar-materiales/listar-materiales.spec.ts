import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarMateriales } from './listar-materiales';

describe('ListarMateriales', () => {
  let component: ListarMateriales;
  let fixture: ComponentFixture<ListarMateriales>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarMateriales]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarMateriales);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
