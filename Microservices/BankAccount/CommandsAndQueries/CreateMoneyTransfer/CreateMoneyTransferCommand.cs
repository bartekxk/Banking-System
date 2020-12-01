﻿using BankAccountService.Common.Exceptions;
using BankAccountService.Common.Models;
using BankAccountService.Data;
using BankAccountService.Data.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BankAccountService.CommandsAndQueries.CreateTransfer
{
    public class CreateMoneyTransferCommand : IRequest<ResponseModel>
    {
        public Guid AccountId { get; private set; }
        public MoneyTransferModel Transfer { get; private set; }

        public CreateMoneyTransferCommand(Guid accountId, MoneyTransferModel transfer)
        {
            AccountId = accountId;
            Transfer = transfer;
        }

        public class CreateTransferCommandHandler : IRequestHandler<CreateMoneyTransferCommand, ResponseModel>
        {
            private readonly DataContext dataContext;

            public CreateTransferCommandHandler(DataContext dataContext)
            {
                this.dataContext = dataContext;
            }

            public async Task<ResponseModel> Handle(CreateMoneyTransferCommand request, CancellationToken cancellationToken)
            {
                var accountFrom = dataContext.BankAccounts.Find(request.AccountId);
                var accountTo = dataContext.BankAccounts.Find(request.Transfer.TargetAccountId);
                if (accountFrom == null || accountTo == null)
                {
                    throw new BankAccountNotFoundException();
                }
                if(request.Transfer.Amount > accountFrom.Balance)
                {
                    throw new BalanceTooLowException();
                }
                accountFrom.Balance -= request.Transfer.Amount;
                accountTo.Balance += request.Transfer.Amount;
                var newMoneyTransfer = new MoneyTransfer(accountFrom.Id, accountTo.Id, request.Transfer.Amount);
                dataContext.MoneyTransfers.Add(newMoneyTransfer);
                dataContext.BankAccounts.Update(accountFrom);
                dataContext.BankAccounts.Update(accountTo);
                dataContext.SaveChanges();

                return new CreateMoneyTransferResponse(HttpStatusCode.OK);
            }
        }
    }
}
