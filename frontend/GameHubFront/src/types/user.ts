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