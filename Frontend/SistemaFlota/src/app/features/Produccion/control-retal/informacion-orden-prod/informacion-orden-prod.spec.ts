import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InformacionOrdenProd } from './informacion-orden-prod';

describe('InformacionOrdenProd', () => {
  let component: InformacionOrdenProd;
  let fixture: ComponentFixture<InformacionOrdenProd>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InformacionOrdenProd]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InformacionOrdenProd);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
