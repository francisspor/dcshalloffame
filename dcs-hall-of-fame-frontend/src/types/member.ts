export enum MemberCategory {
  Staff = 0,
  Alumni = 1
}

export interface HallOfFameMember {
  id: string;
  name: string;
  category: MemberCategory;
  inductionYear: number;
  graduationYear?: number;
  biography: string;
  imageUrl: string;
  achievements: string[];
  createdAt: string; // ISO date string
  updatedAt: string; // ISO date string
}

export interface ApiResponse<T> {
  data?: T;
  error?: string;
}