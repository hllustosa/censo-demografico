import type {
  Address,
  AuthResponse,
  CensusRole,
  CreatePersonInput,
  CreateUserRequest,
  Education,
  PageResult,
  Person,
  PersonCategoryCounter,
  PersonFamilyTree,
  PersonPerCityCounter,
  Race,
  Sex,
  UpdateUserRequest,
  UserListItem,
  UserProfile,
} from "./types";

export type {
  Address,
  AuthResponse,
  CensusRole,
  CreatePersonInput,
  CreateUserRequest,
  Education,
  PageResult,
  Person,
  PersonCategoryCounter,
  PersonFamilyTree,
  PersonPerCityCounter,
  Race,
  Sex,
  UpdateUserRequest,
  UserListItem,
  UserProfile,
};

export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
  correlationId?: string;
  service?: string;
};
