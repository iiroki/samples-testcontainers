import { Client } from 'pg'
import { Item } from './model'

export interface DatabaseService {
  readonly table: string

  readonly init: () => Promise<void>

  readonly destroy: () => Promise<void>

  readonly migrate: () => Promise<void>

  readonly get: () => Promise<Item[]>

  /** Returns true if the item was inserted. */
  readonly insert: (item: Item) => Promise<boolean>
}

export class PostgresService implements DatabaseService {
  private readonly client: Client

  readonly table = 'data'

  constructor(connection: string) {
    this.client = new Client({ connectionString: connection })
  }

  async init() {
    await this.client.connect()
  }

  async destroy() {
    await this.client.end()
  }

  async migrate() {
    const query = `
      CREATE TABLE IF NOT EXISTS ${this.table} (
        key VARCHAR(255) PRIMARY KEY,
        value TEXT NOT NULL,
        timestamp TIMESTAMPTZ
      );
    `

    await this.client.query(query)
  }

  async get() {
    const query = `SELECT * FROM ${this.table};`
    const res = await this.client.query(query)
    return res.rows
  }

  async insert(item: Item) {
    const query = `
      INSERT INTO ${this.table}
      VALUES ($1, $2, $3)
      ON CONFLICT DO NOTHING;
    `

    const res = await this.client.query(query, [item.key, item.value, item.timestamp ?? null])
    return !!res.rowCount && res.rowCount > 0
  }
}
