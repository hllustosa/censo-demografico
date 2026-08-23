import { api } from "./client";
import type {
  AuthResponse,
  CreatePersonInput,
  CreateUserRequest,
  CreatedPerson,
  PageResult,
  PagedUsersResponse,
  Person,
  PersonCategoryCounter,
  PersonFamilyTree,
  PersonPerCityCounter,
  UpdateUserRequest,
  UserProfile,
} from "./types";

export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>("/auth/api/v1/auth/login", { email, password }),
  refresh: (refreshToken: string) =>
    api.post<AuthResponse>("/auth/api/v1/auth/refresh", { refreshToken }),
  logout: (refreshToken: string) =>
    api.post("/auth/api/v1/auth/logout", { refreshToken }),
  me: () => api.get<UserProfile>("/auth/api/v1/auth/me"),
};

export const peopleApi = {
  list: (page: number, name = "") =>
    api.get<PageResult<Person>>("/person/api/v1/person", {
      params: { page, name },
    }),
  get: (id: string) => api.get<Person>(`/person/api/v1/person/${id}`),
  create: (body: CreatePersonInput) =>
    api.post<CreatedPerson>("/person/api/v1/person/", body),
  update: (id: string, body: CreatePersonInput) =>
    api.put(`/person/api/v1/person/${id}`, body),
  remove: (id: string) => api.delete(`/person/api/v1/person/${id}`),
};

export const statsApi = {
  personCategory: (params: {
    name?: string;
    sex?: string;
    education?: string;
    race?: string;
  }) =>
    api.get<PersonCategoryCounter[]>("/stats/api/v1/personcategory", {
      params,
    }),
  cities: () => api.get<string[]>("/stats/api/v1/percitycategory/cities"),
  cityCounter: (city: string) =>
    api.get<PersonPerCityCounter>(
      `/stats/api/v1/percitycategory/cities/${encodeURIComponent(city)}/counter`
    ),
};

export const familyApi = {
  getTree: (personId: string, level: number) =>
    api.get<PersonFamilyTree>(
      `/family/api/v1/familytree/${personId}`,
      { params: { level } }
    ),
};

export const usersApi = {
  list: (page = 1, pageSize = 20) =>
    api.get<PagedUsersResponse>("/auth/api/v1/users", {
      params: { page, pageSize },
    }),
  create: (body: CreateUserRequest) =>
    api.post<UserProfile>("/auth/api/v1/users", body),
  update: (id: string, body: UpdateUserRequest) =>
    api.put<UserProfile>(`/auth/api/v1/users/${id}`, body),
  resetPassword: (id: string, password: string) =>
    api.put(`/auth/api/v1/users/${id}/password`, { password }),
  deactivate: (id: string) => api.delete(`/auth/api/v1/users/${id}`),
};
