import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { familyApi, peopleApi, statsApi, usersApi } from "@/shared/api/endpoints";
import type {
  CreatePersonInput,
  CreateUserRequest,
  PersonFamilyTree,
  UpdateUserRequest,
} from "@/shared/api/types";

export const queryKeys = {
  people: (page: number, name: string) => ["people", page, name] as const,
  person: (id: string) => ["person", id] as const,
  stats: (filters: Record<string, string>) => ["stats", filters] as const,
  cities: ["cities"] as const,
  cityCounter: (city: string) => ["cityCounter", city] as const,
  familyTree: (id: string, level: number) => ["familyTree", id, level] as const,
  users: (page: number) => ["users", page] as const,
};

export function usePeople(page: number, name: string) {
  return useQuery({
    queryKey: queryKeys.people(page, name),
    queryFn: async () => (await peopleApi.list(page, name)).data,
    placeholderData: (prev) => prev,
  });
}

export function usePerson(id?: string) {
  return useQuery({
    queryKey: queryKeys.person(id ?? ""),
    queryFn: async () => (await peopleApi.get(id!)).data,
    enabled: Boolean(id),
  });
}

export function useCreatePerson() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreatePersonInput) => peopleApi.create(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["people"] }),
  });
}

export function useUpdatePerson() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: CreatePersonInput }) =>
      peopleApi.update(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["people"] });
      qc.invalidateQueries({ queryKey: ["person"] });
    },
  });
}

export function useDeletePerson() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => peopleApi.remove(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["people"] }),
  });
}

export function usePersonCategoryStats(filters: {
  name?: string;
  sex?: string;
  education?: string;
  race?: string;
}) {
  return useQuery({
    queryKey: queryKeys.stats(filters as Record<string, string>),
    queryFn: async () => (await statsApi.personCategory(filters)).data,
    placeholderData: (prev) => prev,
  });
}

export function useCities() {
  return useQuery({
    queryKey: queryKeys.cities,
    queryFn: async () => (await statsApi.cities()).data,
  });
}

export function useCityCounter(city?: string) {
  return useQuery({
    queryKey: queryKeys.cityCounter(city ?? ""),
    queryFn: async () => (await statsApi.cityCounter(city!)).data,
    enabled: Boolean(city),
    placeholderData: (prev) => prev,
  });
}

export function useFamilyTree(personId?: string, level = 2) {
  return useQuery({
    queryKey: queryKeys.familyTree(personId ?? "", level),
    queryFn: async () => {
      const { data } = await familyApi.getTree(personId!, level);
      return {
        nodes: data.nodes ?? (data as { Nodes?: PersonFamilyTree["nodes"] }).Nodes ?? [],
        relationships:
          data.relationships ??
          (data as { Relationships?: PersonFamilyTree["relationships"] }).Relationships ??
          [],
      } satisfies PersonFamilyTree;
    },
    enabled: Boolean(personId),
    placeholderData: (prev) => prev,
  });
}

export function useUsers(page: number) {
  return useQuery({
    queryKey: queryKeys.users(page),
    queryFn: async () => (await usersApi.list(page)).data,
    placeholderData: (prev) => prev,
  });
}

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateUserRequest) => usersApi.create(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

export function useUpdateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdateUserRequest }) =>
      usersApi.update(id, body),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

export function useDeactivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}
