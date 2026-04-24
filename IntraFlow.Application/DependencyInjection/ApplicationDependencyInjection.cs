using IntraFlow.Application.Audit.Queries.GetAuditEntriesForEntity;
using IntraFlow.Application.Requests.Commands.AddAttachment;
using IntraFlow.Application.Requests.Commands.AddComment;
using IntraFlow.Application.Requests.Commands.ApproveRequest;
using IntraFlow.Application.Requests.Commands.CancelRequest;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.DeleteAttachment;
using IntraFlow.Application.Requests.Commands.RejectRequest;
using IntraFlow.Application.Requests.Commands.StartReview;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Application.Requests.Queries.ApproverRequests;
using IntraFlow.Application.Requests.Queries.GetAttachments;
using IntraFlow.Application.Requests.Queries.GetComments;
using IntraFlow.Application.Requests.Queries.GetRequestDetails;
using IntraFlow.Application.Requests.Queries.MyRequests;
using IntraFlow.Application.RequestTypes.Commands.CreateRequestType;
using IntraFlow.Application.RequestTypes.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.DependencyInjection
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // services.AddScoped<IExampleService, ExampleService>();

            services.AddScoped<SubmitRequestHandler>();
            services.AddScoped<AddRequestAttachmentHandler>();
            services.AddScoped<AddRequestCommentHandler>();
            services.AddScoped<ApproveRequestHandler>();
            services.AddScoped<CancelRequestHandler>();
            services.AddScoped<CreateRequestHandler>();
            services.AddScoped<DeleteAttachmentHandler>();
            services.AddScoped<RejectRequestHandler>();
            services.AddScoped<StartReviewHandler>();
            services.AddScoped<CreateRequestTypeHandler>();
            services.AddScoped<GetRequestTypesHandler>();
            services.AddScoped<GetMyRequestsHandler>();
            services.AddScoped<GetRequestDetailsHandler>();
            services.AddScoped<GetRequestCommentsHandler>();
            services.AddScoped<GetRequestsForApproverHandler>();
            services.AddScoped<GetAuditEntriesForEntityHandler>();
            services.AddScoped<GetAttachmentFileHandler>();
            services.AddScoped<GetRequestAttachmentsHandler>();
            return services;
        }
    }
}
