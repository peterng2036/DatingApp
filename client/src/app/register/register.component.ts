import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  maxDate: Date = new Date();
  validationErrors: string[] = [];
  registerForm: FormGroup = this.fb.group({
    gender: ['male'],
    username: ['', [Validators.required]],
    KnownAs: ['', [Validators.required]],
    dateOfBirth: ['', [Validators.required]],
    city: ['', [Validators.required]],
    country: ['', [Validators.required]],
    password: [
      '',
      [Validators.required, Validators.minLength(4), Validators.maxLength(8)],
    ],
    confirmPassword: ['', [Validators.required, this.matchValues('password')]],
  });

  matchValues(matchTo: string) {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value
        ? null
        : { notMatching: true };
    };
  }

  constructor(
    private accountService: AccountService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () =>
        this.registerForm.controls['confirmPassword'].updateValueAndValidity(),
    });
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  register() {
    const values = {
      ...this.registerForm.value,
      dateOfBirth: this.getDateOnly(
        this.registerForm.controls['dateOfBirth'].value
      ),
    };

    this.accountService.register(values).subscribe({
      next: () => {
        this.router.navigateByUrl('/members');
      },
      error: (error) => (this.validationErrors = error),
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

  private getDateOnly(dob: string | undefined) {
    if (!dob) return;
    let theDob = new Date(dob);
    return new Date(theDob.getFullYear(), theDob.getMonth(), theDob.getDate())
      .toISOString()
      .slice(0, 10);
  }
}
