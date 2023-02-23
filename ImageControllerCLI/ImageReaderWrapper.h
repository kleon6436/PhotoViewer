/*!
 * @file	ImageReader.h
 * @author	kleon6436
 */

#pragma once

#include "ImageReader.h"
#include "ImageDataWrapper.h"
#include "ImageReaderSettingsWrapper.h"

using namespace Kchary::ImageController::Library;

public ref class ImageReaderWrapper
{
public:
	/*!
	* @brief コンストラクタ
	*/
	ImageReaderWrapper();

	/*!
	* @brief アンマネージド、マネージドリソースの開放
	*/
	~ImageReaderWrapper();

	/*!
	* @brief アンマネージドリソースの解放
	*/
	!ImageReaderWrapper();

	/// <summary>
	/// 画像を取得する
	/// </summary>
	/// <param name="imagePath">ファイルパス</param>
	/// <param name="imageReaderSettings">画像読み込み設定</param>
	/// <param name="imageData">画像データ</param>
	/// <returns>成否</returns>
	System::Boolean GetImageData(System::String^ imagePath, ImageReaderSettingsWrapper^ imageReaderSettings, ImageDataWrapper^ imageData);

private:
	ImageReader *m_imageReaderPtr;		//!< 画像リーダーのポインタ
};

