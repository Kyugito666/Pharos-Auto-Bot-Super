// Modul untuk semua logika bot
pub mod autostaking;
// Tambahkan modul bot lain di sini saat Anda membuatnya
// pub mod r2;
// pub mod brokex;

use crate::pharos::{BotResponse, bot_response};
use tokio::sync::mpsc::Sender;
use tonic::Status;
use chrono::Local;

pub use bot_response::LogLevel;

pub async fn log_to_client(tx: Sender<Result<BotResponse, Status>>, level: LogLevel, message: &str) {
    let now = Local::now();
    let _ = tx.send(Ok(BotResponse {
        timestamp: now.format("%H:%M:%S").to_string(),
        message: message.to_string(),
        level: level.into(),
    })).await;
}
