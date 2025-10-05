// Author: Kyugito666
// Original Source Code from GitHub user: linuxdil

use crate::pharos::{BotRequest, BotResponse};
use crate::bots::{log_to_client, LogLevel};
use ethers::prelude::*;
use std::str::FromStr;
use std::sync::Arc;
use tokio::sync::mpsc::Sender;
use tonic::Status;

abigen!(
    ERC20,
    r#"[
        function approve(address spender, uint256 amount) external returns (bool)
        function balanceOf(address account) external view returns (uint256)
    ]"#,
);

pub async fn run(req: BotRequest, tx: Sender<Result<BotResponse, Status>>) -> Result<(), anyhow::Error> {
    log_to_client(tx.clone(), LogLevel::INFO, "Memulai bot AutoStaking...").await;

    let rpc_url = "https://testnet.dplabs-internal.com/";
    let provider = Provider::<Http>::try_from(rpc_url)?;
    let client = Arc::new(provider);

    for key in req.private_keys {
        let wallet: LocalWallet = key.parse()?;
        let address = wallet.address();
        log_to_client(tx.clone(), LogLevel::INFO, &format!("Memproses wallet: {:?}", address)).await;

        // Contoh interaksi: Cek saldo USDC
        let usdc_address = Address::from_str("0x72df0bcd7276f2dFbAc900D1CE63c272C4BCcCED")?;
        let usdc_contract = ERC20::new(usdc_address, client.clone());
        let balance = usdc_contract.balance_of(address).call().await?;

        log_to_client(tx.clone(), LogLevel::SUCCESS, &format!("Saldo USDC: {} (wei)", balance)).await;

        // Di sini Anda akan menambahkan logika lengkap dari skrip Python:
        // 1. Membaca parameter dari `req.parameters`
        // 2. Melakukan approval token
        // 3. Memanggil fungsi `stake` pada smart contract
        // 4. Menggunakan proxy jika ada di `req.proxies`
        // 5. Menangani delay antar transaksi
    }

    log_to_client(tx.clone(), LogLevel::SUCCESS, "Bot AutoStaking selesai.").await;
    Ok(())
}
