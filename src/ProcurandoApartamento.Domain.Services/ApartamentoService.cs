using System.Threading.Tasks;
using JHipsterNet.Core.Pagination;
using ProcurandoApartamento.Domain.Services.Interfaces;
using ProcurandoApartamento.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace ProcurandoApartamento.Domain.Services
{
    public class ApartamentoService : IApartamentoService
    {
        protected readonly IApartamentoRepository _apartamentoRepository;

        public ApartamentoService(IApartamentoRepository apartamentoRepository)
        {
            _apartamentoRepository = apartamentoRepository;
        }

        public virtual async Task<Apartamento> Save(Apartamento apartamento)
        {
            await _apartamentoRepository.CreateOrUpdateAsync(apartamento);
            await _apartamentoRepository.SaveChangesAsync();
            return apartamento;
        }

        public virtual async Task<IPage<Apartamento>> FindAll(IPageable pageable)
        {
            var page = await _apartamentoRepository.QueryHelper()
                .GetPageAsync(pageable);
            return page;
        }

        public virtual async Task<Apartamento> FindOne(long id)
        {
            var result = await _apartamentoRepository.QueryHelper()
                .GetOneAsync(apartamento => apartamento.Id == id);
            return result;
        }

        public virtual async Task Delete(long id)
        {
            await _apartamentoRepository.DeleteByIdAsync(id);
            await _apartamentoRepository.SaveChangesAsync();
        }

        public async Task<string> FindTheBest(string[] estabelecimentos)
        {
            var quadra = 0;

            var result = await _apartamentoRepository.QueryHelper()
                        .Filter(a => a.ApartamentoDisponivel == true
                                  && a.EstabelecimentoExiste == true
                                  && estabelecimentos.Contains(a.Estabelecimento))
                        .GetAllAsync();

            var quadrasComMaisDeUmRegistro = result
            .GroupBy(a => a.Quadra)
            .Where(g => g.Count() > 1).ToList();

            if (quadrasComMaisDeUmRegistro.Any())
            {
                quadra = quadrasComMaisDeUmRegistro
                    .SelectMany(g => g)
                    .OrderBy(a => Array.IndexOf(estabelecimentos, a.Estabelecimento))
                    .FirstOrDefault().Quadra;

                return $"QUADRA {quadra}";
            }

            quadra = estabelecimentos.Count() > 1
                    ? result.OrderBy(a => Array.IndexOf(estabelecimentos, a.Estabelecimento)).FirstOrDefault().Quadra
                    : result.OrderByDescending(a => a.Quadra).FirstOrDefault().Quadra;

            return $"QUADRA {quadra}";
        }
    }
}
