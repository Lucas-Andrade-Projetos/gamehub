export type User = {
  id: string;
  nickname: string;
  email: string;
  token: string;
  imageUrl?: string;
};

export type RegisterCreds = {
  nickname: string;
  email: string;
  password: string;
  confirmPassword: string;
};

export type LoginCreds = {
  email: string;
  password: string;
};

export type UpdateProfileCreds = {
  nickname: string;
  email: string;
  imageBase64: string | null;
  imageChanged: boolean;
};

export type ChangePasswordCreds = {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
};