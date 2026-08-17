import { Component, computed, ElementRef, inject, OnInit, output, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AccountService } from '../../../core/services/account-service';
import { extractErrorMessage } from '../../../core/utils/http-error';

const AVATAR_SIZE = 256;
const JPEG_QUALITY = 0.8;
const MAX_IMAGE_DATA_URL_LENGTH = 700_000;

@Component({
  selector: 'app-profile-edit',
  imports: [FormsModule],
  templateUrl: './profile-edit.html',
  styleUrl: './profile-edit.css',
})
export class ProfileEdit implements OnInit {
  back = output<void>();
  openPassword = output<void>();

  @ViewChild('fileInput') fileInputRef?: ElementRef<HTMLInputElement>;

  private accountService = inject(AccountService);

  nickname = '';
  email = '';

  editingNickname = signal(false);
  editingEmail = signal(false);
  imagePreview = signal<string | null>(null);
  imageCleared = signal(false);
  errorMessage = signal<string | null>(null);
  submitting = signal(false);

  private originalNickname = '';
  private originalEmail = '';

  displayedImageUrl = computed(() => {
    if (this.imageCleared()) return null;
    return this.imagePreview() ?? this.accountService.currentUser()?.imageUrl ?? null;
  });

  hasPhoto = computed(() => !!this.displayedImageUrl());

  ngOnInit() {
    const user = this.accountService.currentUser();
    this.nickname = user?.nickname ?? '';
    this.email = user?.email ?? '';
    this.originalNickname = this.nickname;
    this.originalEmail = this.email;
  }

  dirty(): boolean {
    return (
      this.nickname !== this.originalNickname ||
      this.email !== this.originalEmail ||
      this.imagePreview() !== null ||
      this.imageCleared()
    );
  }

  startEditNickname() {
    this.editingNickname.set(true);
  }

  startEditEmail() {
    this.editingEmail.set(true);
  }

  triggerFileInput() {
    this.fileInputRef?.nativeElement.click();
  }

  removePhoto() {
    this.imagePreview.set(null);
    this.imageCleared.set(true);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    const reader = new FileReader();

    reader.onload = () => {
      const img = new Image();

      img.onload = () => {
        const canvas = document.createElement('canvas');
        canvas.width = AVATAR_SIZE;
        canvas.height = AVATAR_SIZE;

        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        const minSide = Math.min(img.width, img.height);
        const sx = (img.width - minSide) / 2;
        const sy = (img.height - minSide) / 2;
        ctx.drawImage(img, sx, sy, minSide, minSide, 0, 0, AVATAR_SIZE, AVATAR_SIZE);

        const dataUrl = canvas.toDataURL('image/jpeg', JPEG_QUALITY);

        if (dataUrl.length > MAX_IMAGE_DATA_URL_LENGTH) {
          this.errorMessage.set('Imagem muito grande. Tente uma foto menor.');
          return;
        }

        this.errorMessage.set(null);
        this.imageCleared.set(false);
        this.imagePreview.set(dataUrl);
      };

      img.src = reader.result as string;
    };

    reader.readAsDataURL(file);
  }

  onBackClick() {
    if (!this.dirty()) {
      this.back.emit();
      return;
    }

    this.errorMessage.set(null);
    this.submitting.set(true);

    this.accountService
      .updateProfile({
        nickname: this.nickname,
        email: this.email,
        imageBase64: this.imageCleared() ? null : this.imagePreview(),
        imageChanged: this.imagePreview() !== null || this.imageCleared(),
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.back.emit();
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          this.errorMessage.set(extractErrorMessage(error, 'Não foi possível salvar as alterações.'));
        },
      });
  }
}
