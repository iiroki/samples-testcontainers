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

    it('Get items', async () => {
      // Arrange
      await service.migrate()

      const now = new Date()
      const expected: Item[] = [...Array(10).keys()].map(i => ({
        key: `KEY_${i}`,
        value: `VALUE_${i}`,
        timestamp: (() => {
          const now = new Date()
          now.setMinutes((now.getMinutes() + i) % 59)
          return now
        })()
      }))

      await Promise.all(expected.map(i => service.insert(i)))

      // Act
      const actual = await service.get()

      // Assert
      expect(actual).toStrictEqual(expected)
    })

    it('Insert an item', async () => {
      // Arrange
      await service.migrate()
      expect(await service.get()).toHaveLength(0)

      // Act
      const item: Item = {
        key: uuid(),
        value: 'inserted',
        timestamp: new Date()
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
