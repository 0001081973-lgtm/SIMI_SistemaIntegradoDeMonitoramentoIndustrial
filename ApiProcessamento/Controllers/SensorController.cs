using ApiProcessamento.Config;
using ApiProcessamento.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared;

namespace ApiProcessamento.Controllers
{
    /// <summary>
    /// Controller responsável pelo recebimento, listagem e gerenciamento
    /// dos dados coletados pelos sensores industriais.
    /// </summary>
    [ApiController]
    [Route("api/v1/sensores")]
    [Produces("application/json")]
    public class SensorController : ControllerBase
    {
        private readonly SensorDbContext _db;
        private readonly IOptions<ApiConfig> _config;

        public SensorController(SensorDbContext db, IOptions<ApiConfig> config)
        {
            _db = db;
            _config = config;
        }

        /// <summary>
        /// Recebe e persiste um novo registro de sensor.
        /// </summary>
        /// <param name="sensor">Objeto com os dados do sensor.</param>
        /// <returns>Retorna 201 Created com o registro criado, ou 400 se os dados forem inválidos.</returns>
        /// <response code="201">Dado criado com sucesso.</response>
        /// <response code="400">Dados fora dos limites configurados.</response>
        [HttpPost]
        [ProducesResponseType(typeof(SensorData), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Receber([FromBody] SensorData sensor)
        {
            var cfg = _config.Value;

            if (sensor.Temperatura > cfg.MaxTemperatura)
                return BadRequest($"Temperatura {sensor.Temperatura}°C acima do limite de {cfg.MaxTemperatura}°C.");

            if (sensor.Pressao > cfg.MaxPressao)
                return BadRequest($"Pressão {sensor.Pressao} bar acima do limite de {cfg.MaxPressao} bar.");

            if (sensor.Vibracao > cfg.MaxVibracao)
                return BadRequest($"Vibração {sensor.Vibracao} m/s² acima do limite de {cfg.MaxVibracao} m/s².");

            sensor.Timestamp = DateTime.UtcNow;

            _db.Sensores.Add(sensor);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = sensor.Id }, sensor);
        }

        /// <summary>
        /// Retorna todos os registros de sensores persistidos.
        /// </summary>
        /// <returns>Lista de todos os dados de sensores.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SensorData>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar()
        {
            var dados = await _db.Sensores.OrderByDescending(s => s.Timestamp).ToListAsync();
            return Ok(dados);
        }

        /// <summary>
        /// Retorna um registro de sensor pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único do registro.</param>
        /// <returns>Dado do sensor correspondente ao ID.</returns>
        /// <response code="200">Registro encontrado.</response>
        /// <response code="404">Registro não encontrado.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SensorData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var sensor = await _db.Sensores.FindAsync(id);
            if (sensor is null)
                return NotFound($"Registro com ID {id} não encontrado.");
            return Ok(sensor);
        }

        /// <summary>
        /// Retorna os registros filtrados por origem (ex: "Simulator", "Interface").
        /// </summary>
        /// <param name="origem">Origem dos dados do sensor.</param>
        /// <returns>Lista filtrada por origem.</returns>
        /// <response code="200">Lista retornada com sucesso.</response>
        [HttpGet("por-origem/{origem}")]
        [ProducesResponseType(typeof(IEnumerable<SensorData>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPorOrigem(string origem)
        {
            var dados = await _db.Sensores
                .Where(s => s.Origem.ToLower() == origem.ToLower())
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
            return Ok(dados);
        }

        /// <summary>
        /// Retorna o último registro recebido.
        /// </summary>
        /// <returns>Dado mais recente do sensor.</returns>
        /// <response code="200">Registro retornado.</response>
        /// <response code="404">Nenhum registro encontrado.</response>
        [HttpGet("ultimo")]
        [ProducesResponseType(typeof(SensorData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Ultimo()
        {
            var sensor = await _db.Sensores.OrderByDescending(s => s.Timestamp).FirstOrDefaultAsync();
            if (sensor is null)
                return NotFound("Nenhum dado de sensor registrado.");
            return Ok(sensor);
        }

        /// <summary>
        /// Remove um registro de sensor pelo ID.
        /// </summary>
        /// <param name="id">Identificador único do registro a remover.</param>
        /// <returns>204 No Content em caso de sucesso.</returns>
        /// <response code="204">Registro removido com sucesso.</response>
        /// <response code="404">Registro não encontrado.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deletar(int id)
        {
            var sensor = await _db.Sensores.FindAsync(id);
            if (sensor is null)
                return NotFound($"Registro com ID {id} não encontrado.");

            _db.Sensores.Remove(sensor);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
