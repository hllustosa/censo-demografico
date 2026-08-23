import type { CensusRole, Education, Race, Sex } from "@/shared/lib/constants";

export type Address = {
  zipCode: string;
  addressDesc: string;
  complement: string;
  burrow: string;
  city: string;
  state: string;
};

export type Person = {
  id: string;
  name: string;
  sex: Sex;
  race: Race;
  education: Education;
  address: Address;
  fatherId?: string | null;
  motherId?: string | null;
};

export type PageResult<T> = {
  items: T[];
  page: number;
  totalItems: number;
};

export type CreatePersonInput = {
  id?: string;
  name: string;
  sex: Sex;
  race: Race;
  education: Education;
  address: Address;
  fatherId?: string | null;
  motherId?: string | null;
};

export type CreatedPerson = { id: string };

export type PersonNameCounter = {
  name: string;
  count: number;
};

export type PersonCategoryCounter = {
  id: string;
  race: string;
  schoolLevel: string;
  sex: string;
  count: number;
  personNameCounters: Record<string, PersonNameCounter>;
};

export type PersonPerCityCounter = {
  id: string;
  city: string;
  count: number;
  personNameCounters: Record<string, PersonNameCounter>;
};

export type PersonFamilyTreeNode = {
  id: string;
  name: string;
  fatherId?: string | null;
  motherId?: string | null;
};

export type PersonFamilyTreeRelationship = {
  parentId: string;
  childId: string;
};

export type PersonFamilyTree = {
  nodes: PersonFamilyTreeNode[];
  relationships: PersonFamilyTreeRelationship[];
};

export type UserProfile = {
  id: string;
  email: string;
  fullName: string;
  roles: CensusRole[];
};

export type AuthResponse = {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserProfile;
};

export type UserListItem = {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  isActive: boolean;
  createdAt: string;
};

export type PagedUsersResponse = {
  items: UserListItem[];
  page: number;
  totalItems: number;
};

export type CreateUserRequest = {
  email: string;
  password: string;
  fullName: string;
  roles: CensusRole[];
};

export type UpdateUserRequest = {
  fullName: string;
  roles: CensusRole[];
  isActive: boolean;
};

export type { CensusRole, Education, Race, Sex };
