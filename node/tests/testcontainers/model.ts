export interface TestcontainersLifeCycle {
  readonly create: () => Promise<void>
  readonly destroy: () => Promise<void>
  readonly clean: () => Promise<void>
}
