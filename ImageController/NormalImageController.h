/**
 * @file	NormalImageController.h
 * @author	kleon6436
 */

#pragma once

#include "IImageController.h"
#include <opencv2/opencv.hpp>

namespace Kchary::ImageController::NormalImageControl
{
	class NormalImageController final : public IImageController
	{
	public:
		/*!
		 * @brief コンストラクタ
		 */
		NormalImageController() = default;

		/*!
		* @brief デストラクタ
		*/
		~NormalImageController() = default;

		/*!
		* @brief 画像を読み込み、画像サイズを取得する
		* @param path: 画像ファイルパス
		* @param imageReadSettings	画像読み込み設定
		* @param imageSize	画像サイズ(out)
		* @return 成功: True, 失敗: False
		*/
		bool LoadImageAndGetImageSize(const wchar_t* path, const ImageReadSettings& imageReadSettings, int& imageSize) override;

		/*!
		 * @brief	画像データを取得する
		 * @param	imageData: 画像データ
		 * @return	成功: True, 失敗: False
		 */
		bool GetImageData(ImageData& imageData) override;

		/*!
		 * @brief	画像のサムネイルデータを取得する
		 * @param	imageData: 画像データ
		 * @return	成功: True, 失敗: False
		 */
		bool GetThumbnailImageData(ImageData& imageData) override;

	private:
		/*!
		 * @brief    画像取得モード(OpenCV)を取得する
		 * @param    resizeLongSideLength: リサイズする長辺の長さ
		 * @return    ImreadModes
		 */
		static cv::ImreadModes GetImreadMode(const int resizeLongSideLength);

		/*!
		 * @brief	wcharを文字列(string)に変換する
		 * @param	imagePath: wchar配列の画像ファイルパス
		 * @return 画像ファイルパスの文字列
		 */
		std::string ConvertWcharToString(const wchar_t* imagePath);

		cv::Mat m_image;	//!< 画像
	};
}