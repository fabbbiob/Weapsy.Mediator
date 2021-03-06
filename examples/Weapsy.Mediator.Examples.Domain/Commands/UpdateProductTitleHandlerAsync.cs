﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weapsy.Mediator.Domain;

namespace Weapsy.Mediator.Examples.Domain.Commands
{
    public class UpdateProductTitleHandlerAsync : IDomainCommandHandlerAsync<UpdateProductTitle>
    {
        private readonly IRepository<Product> _repository;

        public UpdateProductTitleHandlerAsync(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<IDomainEvent>> HandleAsync(UpdateProductTitle command)
        {
            var product = await _repository.GetByIdAsync(command.AggregateRootId);

            if (product == null)
                throw new ApplicationException("Product not found.");

            product.UpdateTitle(command.Title);

            return product.Events;
        }
    }
}
