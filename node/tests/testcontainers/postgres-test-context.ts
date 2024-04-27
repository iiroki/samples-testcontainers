import { GenericContainer, StartedTestContainer, TestContainer } from 'testcontainers'
import { TestcontainersLifeCycle } from './model'
import { Client } from 'pg'

export class PostgresTestContext implements TestcontainersLifeCycle {
  private readonly dbPort = 5432
  private container?: StartedTestContainer

  readonly dbName = '_testcontainers'
  readonly dbPassword = 'Passw0rd!'

  get connection() {
    const port = this.container?.getMappedPort(this.dbPort)
    if (!port) {
      throw new Error('Mapped database port not available')
    }

    return `postgres://postgres:${this.dbPassword}@localhost:${port}/${this.dbName}`
  }

  async create() {
    this.container = await new GenericContainer('postgres:16')
      .withEnvironment({ 'POSTGRES_DB': this.dbName, 'POSTGRES_PASSWORD': this.dbPassword })
      .withExposedPorts(this.dbPort)
      .start()
  }

  async destroy() {
    await this.container?.stop()
  }

  async clean() {
    const client = this.getClient()
    await client.connect()

    const tables = await this.getTables(client)
    for (const t of tables) {
      const query = `DROP TABLE ${t};`
      await client.query(query)
    }

    await client.end()
  }

  getClient() {
    return new Client({ connectionString: this.connection })
  }

  async getTables(client?: Client) {
    const useOneShotClient = !client
    const usedClient = useOneShotClient ? this.getClient() : client
    if (useOneShotClient) {
      await usedClient.connect()
    }

    var query = `
      SELECT tablename
      FROM pg_catalog.pg_tables
      WHERE schemaname != 'pg_catalog' AND  schemaname != 'information_schema';
    `

    var res = await usedClient.query(query)

    if (useOneShotClient) {
      await usedClient.end()
    }

    return res.rows.map(r => r.tablename)
  }
}
