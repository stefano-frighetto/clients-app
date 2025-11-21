import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ClientForm } from './client-form';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { vi } from 'vitest';

describe('ClientFormComponent', () => {
  let component: ClientForm;
  let fixture: ComponentFixture<ClientForm>;
  
  const dialogRefMock = {
    close: vi.fn()
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClientForm, NoopAnimationsModule],
      providers: [
        { provide: MatDialogRef, useValue: dialogRefMock },
        { provide: MAT_DIALOG_DATA, useValue: null }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClientForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should invoke dialog close on cancel', () => {
    component.onCancel();
    expect(dialogRefMock.close).toHaveBeenCalledWith(null);
  });

  it('form should be invalid when empty', () => {
    expect(component.form.valid).toBeFalsy();
  });

  it('should close with data if form is valid and saved', () => {
    component.form.controls['firstName'].setValue('Test');
    component.form.controls['lastName'].setValue('User');
    component.form.controls['email'].setValue('test@test.com');
    component.form.controls['cellPhone'].setValue('1122334455');
    
    component.form.controls['corporateName'].setValue('Corp');
    component.form.controls['cuit'].setValue('20-12345678-9');
    component.form.controls['birthdate'].setValue('1990-01-01');

    expect(component.form.valid).toBeTruthy();

    component.onSave();
    
    expect(dialogRefMock.close).toHaveBeenCalledWith(expect.objectContaining({
      firstName: 'Test'
    }));
  });
});