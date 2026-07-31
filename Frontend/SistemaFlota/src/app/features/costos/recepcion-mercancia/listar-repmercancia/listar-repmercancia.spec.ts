import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarRepmercancia } from './listar-repmercancia';

describe('ListarRepmercancia', () => {
  let component: ListarRepmercancia;
  let fixture: ComponentFixture<ListarRepmercancia>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarRepmercancia]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarRepmercancia);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
