import { randomUUID as uuid } from 'node:crypto'
import { PostgresService } from '../src/db'
import { Item } from '../src/model'
import { PostgresTestContext } from './testcontainers/postgres-test-context'

describe('Database tests', () => {
  describe('Postgres', () => {
    const context = new PostgresTestContext()
    let service: PostgresService

    beforeAll(async () => await context.create())

    beforeEach(async () => {
      service = new PostgresService(context.connection)
      await service.init()
    })

    afterEach(async () => {
      await service.destroy()
      await context.clean()
    })

    afterAll(async () => await context.destroy())

    it('Migration creates tables', async () => {
      // Act
      await service.migrate()
      const tables = await context.getTables()

      // Assert
      expect(tables.length).toBeGreaterThan(0)
    })

    // TODO: Get test

    it('Item can be inserted', async () => {
      // Arrange
      await service.migrate()
      expect(await service.get()).toHaveLength(0)

      // Act
      const item: Item = {
        key: uuid(),
        value: 'inserted',
        timestamp: new Date
      }

      const wasInserted = await service.insert(item)

      // Assert
      expect(wasInserted).toBe(true)

      const actual = await service.get()
      expect(actual).toHaveLength(1)

      const actualItem = actual[0]
      expect(actualItem).toStrictEqual(item)
    })
  })
})
