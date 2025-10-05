// Author: Kyugito666
// Original Source Code from GitHub user: linuxdil

use tonic::{transport::Server, Request, Response, Status};
use tokio::sync::mpsc;
use tokio_stream::wrappers::ReceiverStream;
use colored::*;

pub mod pharos {
    tonic::include_proto!("pharos");
}

use pharos::pharos_service_server::{PharosService, PharosServiceServer};
use pharos::{BotRequest, BotResponse};

mod bots;

#[derive(Default)]
pub struct MyPharosService {}

#[tonic::async_trait]
impl PharosService for MyPharosService {
    type RunBotStream = ReceiverStream<Result<BotResponse, Status>>;

    async fn run_bot(&self, request: Request<BotRequest>) -> Result<Response<Self::RunBotStream>, Status> {
        let (tx, rx) = mpsc::channel(128);
        let req_data = request.into_inner();

        tokio::spawn(async move {
            let bot_name = req_data.bot_name;
            bots::log_to_client(tx.clone(), bots::LogLevel::INFO, &format!("Menerima permintaan untuk bot: {}", bot_name)).await;

            let result = match bot_name.as_str() {
                "AutoStaking" => bots::autostaking::run(req_data, tx.clone()).await,
                // Tambahkan pemanggilan untuk bot lain di sini
                _ => {
                    bots::log_to_client(tx.clone(), bots::LogLevel::ERROR, &format!("Bot '{}' belum diimplementasikan.", bot_name)).await;
                    Ok(())
                }
            };

            if let Err(e) = result {
                bots::log_to_client(tx.clone(), bots::LogLevel::ERROR, &format!("Error saat menjalankan bot: {}", e)).await;
            }
        });

        Ok(Response::new(ReceiverStream::new(rx)))
    }
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let addr = "[::1]:50051".parse()?;
    let pharos_service = MyPharosService::default();

    println!("{}", "======================================".green());
    println!("{}", "   Pharos Automation Suite Backend ".bold().green());
    println!("{}", "         Author: Kyugito666".cyan());
    println!("{}", "======================================".green());
    println!("Server gRPC berjalan di {}", addr.to_string().yellow());

    Server::builder()
        .add_service(PharosServiceServer::new(pharos_service))
        .serve(addr)
        .await?;

    Ok(())
}
